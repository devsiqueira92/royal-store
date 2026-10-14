using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Payments;
using RoyalStore.Domain.Sales;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly RoyalStoreContext _context;

    private static readonly string CashCode = ((int)FiscalPaymentType.Cash).ToString("D2");
    private static readonly string CreditCode = ((int)FiscalPaymentType.CreditCard).ToString("D2");
    private static readonly string DebitCode = ((int)FiscalPaymentType.DebitCard).ToString("D2");

    public SaleRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Sale sale, CancellationToken ct = default) =>
        await _context.Sales.AddAsync(sale, ct);

    public async Task AddItemsAsync(IEnumerable<SaleItem> items, CancellationToken ct = default) =>
        await _context.SaleItems.AddRangeAsync(items, ct);

    public async Task AddPaymentsAsync(IEnumerable<SalePayment> payments, CancellationToken ct = default) =>
        await _context.SalePayments.AddRangeAsync(payments, ct);

    public async Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        new HashSet<Guid>(await _context.Sales
            .Where(s => ids.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(ct));

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Sales.FindAsync([id], ct);

    public Task UpdateAsync(Sale sale, CancellationToken ct = default)
    {
        _context.Sales.Update(sale);
        return Task.CompletedTask;
    }

    public async Task<ShiftSalesTotals> GetShiftSalesTotalsAsync(Guid cashShiftId, CancellationToken ct = default)
    {
        // Sum and count in one round trip; no row at all when the shift has no sales.
        var salesAggregate = await _context.Sales
            .AsNoTracking()
            .Where(s => s.CashShiftId == cashShiftId)
            .GroupBy(_ => 1)
            .Select(g => new { Total = g.Sum(s => s.TotalAmount), Count = g.Count() })
            .FirstOrDefaultAsync(ct);

        // Classified by the catalog Payment's fiscal code, not SalePayment.Card.Type — that owned
        // column is also named "PaymentType" in the database but holds "credit"/"debit" card detail
        // (039-cash-shift-summary research.md Decision 1).
        var byCode = await (
                from payment in _context.SalePayments.AsNoTracking()
                join sale in _context.Sales.AsNoTracking() on payment.SaleId equals sale.Id
                join method in _context.Payments.AsNoTracking() on payment.PaymentId equals method.Id
                where sale.CashShiftId == cashShiftId
                    && (method.PaymentType == CashCode
                        || method.PaymentType == CreditCode
                        || method.PaymentType == DebitCode)
                group payment by method.PaymentType into g
                select new { Code = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.Code, x => x.Total, ct);

        return new ShiftSalesTotals(
            salesAggregate?.Total ?? 0m,
            byCode.GetValueOrDefault(CashCode),
            byCode.GetValueOrDefault(CreditCode),
            byCode.GetValueOrDefault(DebitCode),
            salesAggregate?.Count ?? 0);
    }

    public async Task<IReadOnlyDictionary<Guid, ShiftSalesTotals>> GetSalesTotalsByShiftAsync(CancellationToken ct = default)
    {
        // Tenant-wide counterpart of GetShiftSalesTotalsAsync for the shift list: same two aggregates,
        // grouped by shift instead of filtered to one, so the list costs two queries for any shift count.
        var salesByShift = await _context.Sales
            .AsNoTracking()
            .Where(s => s.CashShiftId != null)
            .GroupBy(s => s.CashShiftId!.Value)
            .Select(g => new { ShiftId = g.Key, Total = g.Sum(s => s.TotalAmount), Count = g.Count() })
            .ToListAsync(ct);

        var byShiftAndCode = await (
                from payment in _context.SalePayments.AsNoTracking()
                join sale in _context.Sales.AsNoTracking() on payment.SaleId equals sale.Id
                join method in _context.Payments.AsNoTracking() on payment.PaymentId equals method.Id
                where sale.CashShiftId != null
                    && (method.PaymentType == CashCode
                        || method.PaymentType == CreditCode
                        || method.PaymentType == DebitCode)
                group payment by new { ShiftId = sale.CashShiftId!.Value, Code = method.PaymentType } into g
                select new { g.Key.ShiftId, g.Key.Code, Total = g.Sum(p => p.Amount) })
            .ToListAsync(ct);

        var paymentTotals = byShiftAndCode.ToDictionary(x => (x.ShiftId, x.Code), x => x.Total);
        decimal PaymentTotal(Guid shiftId, string code) => paymentTotals.GetValueOrDefault((shiftId, code));

        return salesByShift.ToDictionary(
            s => s.ShiftId,
            s => new ShiftSalesTotals(
                s.Total,
                PaymentTotal(s.ShiftId, CashCode),
                PaymentTotal(s.ShiftId, CreditCode),
                PaymentTotal(s.ShiftId, DebitCode),
                s.Count));
    }
}
