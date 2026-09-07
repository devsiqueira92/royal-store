using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Fiscal;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.SyncSales;

public sealed class SyncSalesCommandHandler
    : IRequestHandler<SyncSalesCommand, Result>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFiscalReceiptQueue _fiscalQueue;

    public SyncSalesCommandHandler(
        ISaleRepository saleRepository,
        ICashShiftRepository cashShiftRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        IFiscalReceiptQueue fiscalQueue)
    {
        _saleRepository = saleRepository;
        _cashShiftRepository = cashShiftRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _fiscalQueue = fiscalQueue;
    }

    public async Task<Result> Handle(
        SyncSalesCommand command,
        CancellationToken cancellationToken)
    {
        //var userId = _currentUserService.CurrentUserId;
        //var tenantId = _tenantContext.TenantId;

        //var submittedIds = command.Sales.Select(s => s.Id).ToList();
        //var existingIds = await _saleRepository.GetExistingIdsAsync(submittedIds, cancellationToken);

        //var uniqueShiftIds = command.Sales.Select(s => s.CashShiftId).Distinct().ToList();
        //var validShiftIds = await _cashShiftRepository.GetExistingIdsAsync(uniqueShiftIds, cancellationToken);

        //var results = new List<SaleSyncItemResult>();
        //var salesToAdd = new List<Sale>();
        //var itemsToAdd = new List<SaleItem>();
        //var paymentsToAdd = new List<SalePayment>();

        //foreach (var req in command.Sales)
        //{
        //    if (existingIds.Contains(req.Id))
        //    {
        //        results.Add(new SaleSyncItemResult(req.Id, SyncStatus.AlreadyRegistered));
        //        continue;
        //    }

        //    if (!validShiftIds.Contains(req.CashShiftId))
        //    {
        //        results.Add(new SaleSyncItemResult(req.Id, SyncStatus.ValidationFailed,
        //            "Cash shift not found or does not belong to this tenant."));
        //        continue;
        //    }

        //    var saleResult = Sale.Create(req.Id, tenantId, req.CashShiftId, userId,
        //        req.TotalAmount, req.DiscountAmount, req.SoldAt, DateTime.UtcNow);

        //    if (!saleResult.IsSuccess)
        //    {
        //        results.Add(new SaleSyncItemResult(req.Id, SyncStatus.ValidationFailed, saleResult.Error.Message));
        //        continue;
        //    }

        //    if (req.Items is null || !req.Items.Any())
        //    {
        //        results.Add(new SaleSyncItemResult(req.Id, SyncStatus.ValidationFailed,
        //            "Sale must have at least one item."));
        //        continue;
        //    }

        //    var items = new List<SaleItem>();
        //    var itemsValid = true;
        //    string? itemError = null;

        //    foreach (var itemReq in req.Items)
        //    {
        //        var itemResult = SaleItem.Create(Guid.NewGuid(), tenantId, req.Id,
        //            itemReq.ProductId, itemReq.ProductName, itemReq.ProductCode,
        //            itemReq.Quantity, itemReq.UnitPrice, itemReq.TotalPrice,
        //            itemReq.Ncm, itemReq.Cest);

        //        if (!itemResult.IsSuccess)
        //        {
        //            itemsValid = false;
        //            itemError = itemResult.Error.Message;
        //            break;
        //        }

        //        items.Add(itemResult.Value);
        //    }

        //    if (!itemsValid)
        //    {
        //        results.Add(new SaleSyncItemResult(req.Id, SyncStatus.ValidationFailed, itemError));
        //        continue;
        //    }

        //    if (req.Payments is null || !req.Payments.Any())
        //    {
        //        results.Add(new SaleSyncItemResult(req.Id, SyncStatus.ValidationFailed,
        //            "Sale must have at least one payment."));
        //        continue;
        //    }

        //    var payments = new List<SalePayment>();
        //    var paymentsValid = true;
        //    string? paymentError = null;

        //    foreach (var payReq in req.Payments)
        //    {
        //        var payResult = SalePayment.Create(Guid.NewGuid(), tenantId, req.Id,
        //            payReq.Method, payReq.Amount);

        //        if (!payResult.IsSuccess)
        //        {
        //            paymentsValid = false;
        //            paymentError = payResult.Error.Message;
        //            break;
        //        }

        //        payments.Add(payResult.Value);
        //    }

        //    if (!paymentsValid)
        //    {
        //        results.Add(new SaleSyncItemResult(req.Id, SyncStatus.ValidationFailed, paymentError));
        //        continue;
        //    }

        //    salesToAdd.Add(saleResult.Value);
        //    itemsToAdd.AddRange(items);
        //    paymentsToAdd.AddRange(payments);
        //}

        //if (salesToAdd.Count > 0)
        //{
        //    try
        //    {
        //        foreach (var sale in salesToAdd)
        //            await _saleRepository.AddAsync(sale, cancellationToken);

        //        await _saleRepository.AddItemsAsync(itemsToAdd, cancellationToken);
        //        await _saleRepository.AddPaymentsAsync(paymentsToAdd, cancellationToken);
        //        await _unitOfWork.SaveChangesAsync(cancellationToken);

        //        foreach (var sale in salesToAdd)
        //            await _fiscalQueue.QueueSaleAsync(sale.Id, cancellationToken);

        //        results.AddRange(salesToAdd.Select(s => new SaleSyncItemResult(s.Id, SyncStatus.Created)));
        //    }
        //    catch (DbUpdateException)
        //    {
        //        // Concurrent duplicate: fall back to individual saves
        //        foreach (var sale in salesToAdd)
        //        {
        //            try
        //            {
        //                var saleItems = itemsToAdd.Where(i => i.SaleId == sale.Id).ToList();
        //                var salePayments = paymentsToAdd.Where(p => p.SaleId == sale.Id).ToList();

        //                await _saleRepository.AddAsync(sale, cancellationToken);
        //                await _saleRepository.AddItemsAsync(saleItems, cancellationToken);
        //                await _saleRepository.AddPaymentsAsync(salePayments, cancellationToken);
        //                await _unitOfWork.SaveChangesAsync(cancellationToken);
        //                await _fiscalQueue.QueueSaleAsync(sale.Id, cancellationToken);

        //                results.Add(new SaleSyncItemResult(sale.Id, SyncStatus.Created));
        //            }
        //            catch (DbUpdateException)
        //            {
        //                results.Add(new SaleSyncItemResult(sale.Id, SyncStatus.AlreadyRegistered));
        //            }
        //        }
        //    }
        //}

        //return Result<SyncSalesResponse>.Success(new SyncSalesResponse(results));
        return Result.Success();
    }
}
