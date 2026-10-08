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
using RoyalStore.Domain.Payments;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.RegisterSale;

public sealed class RegisterSaleCommandHandler
    : IRequestHandler<RegisterSaleCommand, Result<RegisterSaleResponse>>
{
    /// <summary>The catalog's fixed 2-char fiscal code for the cashback/loyalty settlement method
    /// (037-payment-methods-catalog seeds it from <see cref="FiscalPaymentType.CashbackOrLoyalty"/>).
    /// A payment line resolving to this code is a cashback redemption, not real tender (FR-018).</summary>
    private static readonly string CashbackPaymentTypeCode = ((int)FiscalPaymentType.CashbackOrLoyalty).ToString("D2");

    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IIbptTaxRepository _ibptTaxRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ICashbackLedger _cashbackLedger;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFiscalReceiptQueue _fiscalQueue;
    private readonly ICashShiftRepository _cashShiftRepository;

    public RegisterSaleCommandHandler(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IIbptTaxRepository ibptTaxRepository,
        ICustomerRepository customerRepository,
        ICashbackLedger cashbackLedger,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        IFiscalReceiptQueue fiscalQueue,
        ICashShiftRepository cashShiftRepository)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _ibptTaxRepository = ibptTaxRepository;
        _customerRepository = customerRepository;
        _cashbackLedger = cashbackLedger;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _fiscalQueue = fiscalQueue;
        _cashShiftRepository = cashShiftRepository;
    }

    public async Task<Result<RegisterSaleResponse>> Handle(
        RegisterSaleCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // The operator is always the authenticated principal — the new request shape has no
        // seller/operator field at all to even attempt to forge (038-register-sale-checkout FR-006).
        var userId = _currentUserService.CurrentUserId;
        var openShift = await _cashShiftRepository.GetOpenByUserIdAsync(userId, cancellationToken);
        if (openShift is null) 
            return Error.Validation("sale.no_open_shift",
                "The current user does not have an open cash shift and cannot register a sale.");

        var checkout = command.Checkout;

        var fulfillmentModeResult = FulfillmentModeParser.Parse(checkout.Fulfillment.Mode);
        if (fulfillmentModeResult.IsFailure)
            return fulfillmentModeResult.Error;

        var fulfillmentMode = fulfillmentModeResult.Value;
        if (fulfillmentMode == FulfillmentMode.Pickup && string.IsNullOrWhiteSpace(checkout.Fulfillment.BranchId))
            return Error.Validation("sale.branch_required_for_pickup",
                "A branch is required when the fulfillment mode is pickup.");

        // Customer is identified by id only — never by document (FR-008); a request with no
        // matching customer is rejected outright (FR-017).
        Customer? customer = null;
        if (checkout.Customer is not null)
        {
            customer = await _customerRepository.GetByIdAsync(checkout.Customer.Id, cancellationToken);
            if (customer is null || customer.TenantId != tenantId)
                return Error.Validation("sale.customer_not_found",
                    $"Customer {checkout.Customer.Id} was not found.");
        }

        var saleId = Guid.NewGuid();
        var soldAt = DateTime.UtcNow;

        var itemsResult = await BuildItemsAsync(command.Items, tenantId, saleId, soldAt, cancellationToken);
        if (itemsResult.IsFailure)
            return itemsResult.Error;

        var (items, subtotal, taxAmount) = itemsResult.Value;

        if (Math.Abs(subtotal - checkout.Totals.Subtotal) > Sale.ReconciliationTolerance)
            return Error.Validation("sale.subtotal_does_not_match_items",
                "The sum of the sale lines does not match the stated subtotal.");

        // The one genuine client input among the totals: a manual, cashier-entered discount with no
        // other source (research.md Decision 8). Everything downstream is computed server-side.
        var saleDiscount = checkout.Totals.SaleDiscount;
        var baseAmount = Math.Max(subtotal - saleDiscount, 0m);

        var loyaltyDiscount = customer?.LoyaltyTier is not null
            ? Math.Round(baseAmount * customer.LoyaltyTier.CashbackPercentage / 100m, 2)
            : 0m;

        var paymentsResult = await ResolvePaymentsAsync(checkout.Payments, tenantId, cancellationToken);
        if (paymentsResult.IsFailure)
            return paymentsResult.Error;

        var resolvedPayments = paymentsResult.Value;
        var cashbackLines = resolvedPayments.Where(p => p.IsCashback).ToList();
        var tenderLines = resolvedPayments.Where(p => !p.IsCashback).ToList();

        if (cashbackLines.Count > 0 && !checkout.UseCashback)
            return Error.Validation("sale.cashback_not_requested",
                "A cashback settlement line was submitted, but cashback redemption was not requested.");

        if (cashbackLines.Count > 1)
            return Error.Validation("sale.multiple_cashback_lines",
                "A sale can carry at most one cashback settlement line.");

        var tenderAmountSum = tenderLines.Sum(p => p.Line.Amount);
        var paymentDiscount = tenderAmountSum > 0
            ? tenderLines.Sum(p => Math.Round(baseAmount * (p.Line.Amount / tenderAmountSum) * (p.CatalogPayment.Discount / 100m), 2))
            : 0m;

        var amountDue = Math.Max(baseAmount - loyaltyDiscount - paymentDiscount + taxAmount, 0m);

        // Redeemed before Sale.Create because CashbackApplied feeds the earning-base calculation
        // below (a sale never earns cashback on the amount it just redeemed) and CashbackRemaining
        // (data-model.md §Sale) — matches the existing pattern from 036/037.
        var cashbackApplied = 0m;
        // Read before redeeming: RedeemAsync's changes are only tracked in-memory until
        // SaveChangesAsync runs later, so a query issued *after* it would still see the
        // pre-redemption balance from the database. Computing the post-redemption figure
        // arithmetically avoids that staleness entirely.
        var cashbackBalanceBeforeRedemption = customer is not null
            ? await _cashbackLedger.GetUsableBalanceAsync(tenantId, customer.Id, cancellationToken)
            : 0m;

        if (customer is not null && checkout.UseCashback && cashbackLines.Count == 1)
        {
            var requested = cashbackLines[0].Line.Amount;
            var redeemResult = await _cashbackLedger.RedeemAsync(
                tenantId, customer.Id, Math.Min(requested, amountDue), saleId, cancellationToken);
            if (redeemResult.IsFailure)
                return redeemResult.Error;

            cashbackApplied = redeemResult.Value;
        }

        var cashbackRemaining = cashbackBalanceBeforeRedemption - cashbackApplied;

        var totalAmount = amountDue - cashbackApplied;

        if (Math.Abs(tenderAmountSum - totalAmount) > Sale.ReconciliationTolerance)
            return Error.Validation("sale.payments_do_not_settle_total",
                "Payment amounts must equal the sale total.");

        SaleCustomerSnapshot? customerSnapshot = customer is null
            ? null
            : new SaleCustomerSnapshot(customer.Name, customer.LoyaltyTier?.Name ?? string.Empty);

        var saleResult = Sale.Create(saleId, tenantId, openShift.Id, userId, customer?.Id,
            subtotal, saleDiscount, loyaltyDiscount, paymentDiscount, taxAmount, totalAmount,
            cashbackApplied, cashbackRemaining, fulfillmentMode, checkout.Fulfillment.BranchId,
            customerSnapshot, soldAt, DateTime.UtcNow);

        if (saleResult.IsFailure)
            return saleResult.Error;

        var sale = saleResult.Value;

        foreach (var item in items)
        {
            var added = sale.AddItem(item);
            if (added.IsFailure)
                return added.Error;
        }

        var buildPaymentsResult = BuildPayments(resolvedPayments, tenantId, sale, cashbackApplied);
        if (buildPaymentsResult.IsFailure)
            return buildPaymentsResult.Error;

        // Earned after items/payments validate cleanly, so a sale that turns out to be invalid never
        // reaches the ledger. The earning base excludes cashback just redeemed on this same sale.
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

        // Every sale also lands in the shift's movement history (041-sale-cash-movement). Cashback lines
        // are not money received, so only tender lines name the payment methods.
        var paymentMethodNames = tenderLines.Select(p => p.CatalogPayment.PaymentMethod).ToList();
        var saleMovementResult = openShift.RecordSale(Guid.NewGuid(), sale, paymentMethodNames);
        if (saleMovementResult.IsFailure)
            return saleMovementResult.Error;

        try
        {
            // Explicit add (no UpdateAsync on the tracked shift): the movement must be tracked as Added,
            // and the shift's Version bump is picked up by change detection on the same save.
            if (saleMovementResult.Value is not null)
                await _cashShiftRepository.AddMovementAsync(saleMovementResult.Value, cancellationToken);

            await _saleRepository.AddAsync(sale, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex) when (ex.Entries.Any(e => e.Entity is CashShift))
        {
            // The shift changed after it was loaded — closed, or another movement/sale committed — so
            // nothing (sale, movement, cashback) is saved; the operator refreshes and retries (FR-009).
            return Error.Conflict("cashshift.concurrent_update",
                "The cash shift was changed by another operation. Refresh and try again.");
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another terminal consumed the same cashback entry first. The whole sale rolls back with
            // it — nothing here is partially applied — so the caller can safely resubmit the same
            // checkout after refetching the customer's balance (research.md Decision 11).
            return Error.Validation("sale.cashback_conflict",
                "This customer's cashback balance changed concurrently. Refresh their balance and retry.");
        }

        await _fiscalQueue.QueueSaleAsync(sale.Id, cancellationToken);

        return new RegisterSaleResponse(sale.Id, SyncStatus.Created, cashbackApplied, cashbackEarned,
            paymentDiscount, loyaltyDiscount, taxAmount, totalAmount);
    }

    private async Task<Result<(List<SaleItem> Items, decimal Subtotal, decimal TaxAmount)>> BuildItemsAsync(
        IReadOnlyList<RegisterSaleItem> requestItems, Guid tenantId, Guid saleId, DateTime soldAt,
        CancellationToken cancellationToken)
    {
        var variantIds = requestItems.Select(i => i.VariantId).ToList();
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
            ncmCodes, DateOnly.FromDateTime(soldAt), cancellationToken);

        var items = new List<SaleItem>();
        var subtotal = 0m;
        var taxAmount = 0m;

        foreach (var line in requestItems)
        {
            if (!variantIndex.TryGetValue(line.VariantId, out var pair))
                return Error.Validation("sale.variant_not_found",
                    $"Variant {line.VariantId} was not found, is inactive, or does not belong to this store.");

            var (product, variant) = pair;

            if (product.Id != line.ProductId)
                return Error.Validation("sale.variant_product_mismatch",
                    $"Variant {line.VariantId} does not belong to product {line.ProductId}.");

            var ibptTax = ResolveTax(product, ibptByCode);

            var itemResult = SaleItemEnricher.Enrich(line, tenantId, saleId, product, variant, ibptTax);
            if (itemResult.IsFailure)
                return itemResult.Error;

            items.Add(itemResult.Value);
            subtotal += itemResult.Value.TotalPrice;
            taxAmount += ItemTaxAmount(itemResult.Value);
        }

        return (items, subtotal, taxAmount);
    }

    /// <summary>Per-item tax-burden amount: the federal share (national or imported, by origin) plus
    /// state and municipal rates, applied to the line's total — an exact port of the checkout
    /// frontend's own <c>lineItemTaxRate</c> formula (research.md Decision 8), so a receipt computed
    /// by either side agrees.</summary>
    private static decimal ItemTaxAmount(SaleItem item)
    {
        var isNational = item.Fiscal.Origin == "0";
        var federalRate = isNational ? item.TaxRates.NationalRate : item.TaxRates.ImportedFederalRate;
        var rate = (federalRate ?? 0m) + (item.TaxRates.StateRate ?? 0m) + (item.TaxRates.MunicipalRate ?? 0m);
        return Math.Round(item.TotalPrice * rate / 100m, 2);
    }

    private static IbptTax? ResolveTax(Product product, IReadOnlyDictionary<string, IbptTax> ibptByCode) =>
        product.Ncm is not null && ibptByCode.TryGetValue(product.Ncm, out var tax) ? tax : null;

    private sealed record ResolvedPayment(
        RegisterSalePayment Line, Payment CatalogPayment, FiscalPaymentType FiscalType, bool IsCashback);

    private async Task<Result<List<ResolvedPayment>>> ResolvePaymentsAsync(
        IReadOnlyList<RegisterSalePayment> payments, Guid tenantId, CancellationToken cancellationToken)
    {
        var resolved = new List<ResolvedPayment>();

        foreach (var line in payments)
        {
            var catalogPayment = await _paymentRepository.GetByIdAsync(line.MethodId, cancellationToken);
            if (catalogPayment is null || catalogPayment.TenantId != tenantId)
                return Error.Validation("sale.payment_method_invalid",
                    $"Payment method {line.MethodId} was not found or does not belong to this store.");

            if (!catalogPayment.Active)
                return Error.Validation("sale.payment_method_invalid",
                    $"Payment method '{catalogPayment.PaymentMethod}' is no longer active.");

            if (!Enum.IsDefined(typeof(FiscalPaymentType), line.FiscalType))
                return Error.Validation("sale.fiscal_type_invalid",
                    $"Fiscal payment type {line.FiscalType} is not recognised.");

            var fiscalType = (FiscalPaymentType)line.FiscalType;
            var isCashback = catalogPayment.PaymentType == CashbackPaymentTypeCode;

            resolved.Add(new ResolvedPayment(line, catalogPayment, fiscalType, isCashback));
        }

        return resolved;
    }

    private static Result BuildPayments(
        List<ResolvedPayment> resolvedPayments, Guid tenantId, Sale sale, decimal cashbackApplied)
    {
        foreach (var resolved in resolvedPayments)
        {
            // The cashback line's persisted amount is what the ledger actually redeemed — the
            // client-submitted amount for that line is never trusted (FR-018).
            var amount = resolved.IsCashback ? cashbackApplied : resolved.Line.Amount;

            var cardDto = resolved.Line.Card;
            var card = cardDto is null
                ? PaymentCardDetails.Empty
                : new PaymentCardDetails(cardDto.Flag, cardDto.Type, cardDto.Installments);

            var paymentResult = SalePayment.Create(
                Guid.NewGuid(), tenantId, sale.Id, resolved.FiscalType, amount, resolved.CatalogPayment.Id, card);

            if (paymentResult.IsFailure)
                return Result.Failure(paymentResult.Error);

            var added = sale.AddPayment(paymentResult.Value);
            if (added.IsFailure)
                return added;
        }

        return Result.Success();
    }
}
