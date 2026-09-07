using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Fiscal;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Fiscal;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.RegisterSale;

public sealed class RegisterSaleCommandHandler
    : IRequestHandler<RegisterSaleCommand, Result<RegisterSaleResponse>>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly IProductRepository _productRepository;
    private readonly IIbptTaxRepository _ibptTaxRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICashbackLedger _cashbackLedger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFiscalReceiptQueue _fiscalQueue;

    public RegisterSaleCommandHandler(
        ISaleRepository saleRepository,
        ICashShiftRepository cashShiftRepository,
        IProductRepository productRepository,
        IIbptTaxRepository ibptTaxRepository,
        ICustomerRepository customerRepository,
        ICashbackLedger cashbackLedger,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        IFiscalReceiptQueue fiscalQueue)
    {
        _saleRepository = saleRepository;
        _cashShiftRepository = cashShiftRepository;
        _productRepository = productRepository;
        _ibptTaxRepository = ibptTaxRepository;
        _customerRepository = customerRepository;
        _cashbackLedger = cashbackLedger;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _fiscalQueue = fiscalQueue;
    }

    public async Task<Result<RegisterSaleResponse>> Handle(
        RegisterSaleCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // The operator is the authenticated principal. metadata.SellerId is deliberately ignored:
        // a client-supplied attribution would be trivially forgeable.
        var userId = _currentUserService.CurrentUserId;

        var totals = command.Sale;

        var discountMode = DiscountModeParser.Parse(totals.DiscountMode);
        if (discountMode.IsFailure)
            return discountMode.Error;

        var reconciliation = ValidateMonetaryConsistency(command);
        if (reconciliation.IsFailure)
            return reconciliation.Error;

        var saleId = command.Metadata.SaleId ?? Guid.NewGuid();

        if (command.Metadata.SaleId is not null)
        {
            var existing = await _saleRepository.GetExistingIdsAsync([saleId], cancellationToken);
            if (existing.Contains(saleId))
                return new RegisterSaleResponse(saleId, SyncStatus.AlreadyRegistered);
        }

        var shift = await _cashShiftRepository.GetByIdAsync(command.Metadata.CashRegisterSessionId, cancellationToken);
        if (shift is null || shift.ClosedAt is not null)
            return Error.Validation("sale.cash_shift_not_open",
                "The cash register session is unknown, does not belong to this store, or is already closed.");

        var soldAt = command.Metadata.SoldAt ?? DateTime.UtcNow;

        // The document the terminal already sends is the only signal used to identify a customer for
        // loyalty purposes — a checkout never creates one (that stays a manager action, FR-005). No
        // match, or no document at all, makes this an unidentified sale (FR-017): loyalty is simply
        // skipped, checkout is otherwise unaffected (research.md Decision 1).
        Customer? customer = null;
        if (!string.IsNullOrWhiteSpace(command.Customer?.Document))
        {
            var normalizedDocument = DocumentValidator.Normalize(command.Customer.Document);
            customer = await _customerRepository.GetByDocumentAsync(tenantId, normalizedDocument, cancellationToken);
        }

        // Redeemed before Sale.Create because CashbackApplied feeds both the sale record and the
        // earning-base calculation below (FR-016a: cashback redeemed on a sale never earns cashback).
        var cashbackApplied = 0m;
        if (customer is not null && totals.CashbackRequested > 0)
        {
            var redeemResult = await _cashbackLedger.RedeemAsync(
                tenantId, customer.Id, totals.CashbackRequested, saleId, cancellationToken);
            if (redeemResult.IsFailure)
                return redeemResult.Error;

            cashbackApplied = redeemResult.Value;
        }

        var saleResult = Sale.Create(saleId, tenantId, shift.Id, userId, customer?.Id,
            totals.Subtotal, totals.Discount, discountMode.Value, totals.AdditionalFees,
            totals.Total, totals.Change, cashbackApplied, soldAt, DateTime.UtcNow);

        if (saleResult.IsFailure)
            return saleResult.Error;

        var sale = saleResult.Value;

        var itemsResult = await BuildItemsAsync(command, tenantId, sale, cancellationToken);
        if (itemsResult.IsFailure)
            return itemsResult.Error;

        var paymentsResult = BuildPayments(command, tenantId, sale);
        if (paymentsResult.IsFailure)
            return paymentsResult.Error;

        // Earned after items/payments validate cleanly, so a sale that turns out to be invalid never
        // reaches the ledger (FR-016). The earning base excludes AdditionalFees entirely (FR-016a).
        var cashbackEarned = 0m;
        if (customer is not null)
        {
            var earningBase = Math.Max(0m, sale.Subtotal - sale.DiscountAmount - sale.CashbackApplied);
            var accrueResult = await _cashbackLedger.AccrueAsync(
                tenantId, customer.Id, sale.Id, earningBase, sale.SoldAt, cancellationToken);
            if (accrueResult.IsFailure)
                return accrueResult.Error;

            cashbackEarned = accrueResult.Value;
            var recordResult = sale.RecordCashbackEarned(cashbackEarned);
            if (recordResult.IsFailure)
                return recordResult.Error;
        }

        try
        {
            await _saleRepository.AddAsync(sale, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another terminal consumed the same cashback entry first (research.md Decision 5). The
            // whole sale rolls back with it — nothing here is partially applied — so the caller can
            // safely resubmit the same checkout after refetching the customer's balance.
            return Error.Validation("sale.cashback_conflict",
                "This customer's cashback balance changed concurrently. Refresh their balance and retry.");
        }
        catch (DbUpdateException)
        {
            // A concurrent submission of the same idempotency key won the race.
            return new RegisterSaleResponse(saleId, SyncStatus.AlreadyRegistered);
        }

        await _fiscalQueue.QueueSaleAsync(sale.Id, cancellationToken);

        return new RegisterSaleResponse(sale.Id, SyncStatus.Created, cashbackApplied, cashbackEarned);
    }

    private async Task<Result> BuildItemsAsync(
        RegisterSaleCommand command, Guid tenantId, Sale sale, CancellationToken cancellationToken)
    {
        var variantIds = command.Items.Select(i => i.VariantId).ToList();
        var products = await _productRepository.GetForCheckoutAsync(variantIds, cancellationToken);

        var variantIndex = products
            .SelectMany(product => product.Variants.Select(variant => (product, variant)))
            .ToDictionary(pair => pair.variant.Id);

        var ncmCodes = products
            .Select(p => p.Ncm)
            .Where(ncm => !string.IsNullOrWhiteSpace(ncm))
            .Select(ncm => ncm!)
            .Distinct()
            .ToList();

        var ibptByCode = await _ibptTaxRepository.GetCurrentByCodesAsync(
            ncmCodes, DateOnly.FromDateTime(sale.SoldAt), cancellationToken);

        var lineTotal = 0m;

        foreach (var line in command.Items)
        {
            if (!variantIndex.TryGetValue(line.VariantId, out var pair))
                return Result.Failure(Error.Validation("sale.variant_not_found",
                    $"Variant {line.VariantId} was not found, is inactive, or does not belong to this store."));

            var (product, variant) = pair;

            if (product.Id != line.ProductId)
                return Result.Failure(Error.Validation("sale.variant_product_mismatch",
                    $"Variant {line.VariantId} does not belong to product {line.ProductId}."));

            var ibptTax = ResolveTax(product, ibptByCode);

            var itemResult = SaleItemEnricher.Enrich(line, tenantId, sale.Id, product, variant, ibptTax);
            if (itemResult.IsFailure)
                return Result.Failure(itemResult.Error);

            var added = sale.AddItem(itemResult.Value);
            if (added.IsFailure)
                return added;

            lineTotal += itemResult.Value.TotalPrice;
        }

        if (Math.Abs(lineTotal - sale.Subtotal) > Sale.ReconciliationTolerance)
            return Result.Failure(Error.Validation("sale.subtotal_does_not_match_items",
                "The sum of the sale lines does not match the stated subtotal."));

        return Result.Success();
    }

    private static IbptTax? ResolveTax(Product product, IReadOnlyDictionary<string, IbptTax> ibptByCode) =>
        product.Ncm is not null && ibptByCode.TryGetValue(product.Ncm, out var tax) ? tax : null;

    private static Result BuildPayments(RegisterSaleCommand command, Guid tenantId, Sale sale)
    {
        foreach (var payment in command.Payments)
        {
            var method = PaymentMethodMapper.Map(payment.Method, payment.Type);
            if (method.IsFailure)
                return Result.Failure(method.Error);

            var card = new PaymentCardDetails(
                payment.Type?.Trim(),
                payment.Brand?.Trim(),
                payment.Installments,
                payment.AuthCode?.Trim(),
                payment.Nsu?.Trim());

            var paymentResult = SalePayment.Create(
                Guid.NewGuid(), tenantId, sale.Id, method.Value, payment.Amount, card);

            if (paymentResult.IsFailure)
                return Result.Failure(paymentResult.Error);

            var added = sale.AddPayment(paymentResult.Value);
            if (added.IsFailure)
                return added;
        }

        return Result.Success();
    }

    /// <summary>
    /// The terminal's own figures must be internally consistent before anything is stored. A one-cent
    /// tolerance absorbs the rounding a percentage discount produces per line.
    /// </summary>
    private static Result ValidateMonetaryConsistency(RegisterSaleCommand command)
    {
        var totals = command.Sale;

        var settled = command.Payments.Sum(p => p.Amount) - totals.Change;
        if (Math.Abs(settled - totals.Total) > Sale.ReconciliationTolerance)
            return Result.Failure(Error.Validation("sale.payments_do_not_settle_total",
                "Payment amounts less change must equal the sale total."));

        if (totals.Change > 0)
        {
            var hasCash = command.Payments.Any(p =>
                PaymentMethodMapper.Map(p.Method, p.Type) is { IsSuccess: true, Value: PaymentMethod.Money });

            if (!hasCash)
                return Result.Failure(Error.Validation("sale.change_without_cash_payment",
                    "Change can only be returned on a sale settled at least partly in cash."));
        }

        return Result.Success();
    }
}
