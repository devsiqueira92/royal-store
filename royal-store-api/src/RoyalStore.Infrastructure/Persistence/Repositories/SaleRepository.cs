using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Payments;
using RoyalStore.Domain.Sales;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly RoyalStoreContext _context;

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
        var totalSales = await _context.Sales
            .AsNoTracking()
            .Where(s => s.CashShiftId == cashShiftId)
            .SumAsync(s => s.TotalAmount, ct);

        // Classified by the catalog Payment's fiscal code, not SalePayment.Card.Type — that owned
        // column is also named "PaymentType" in the database but holds "credit"/"debit" card detail
        // (039-cash-shift-summary research.md Decision 1).
        var cashCode = ((int)FiscalPaymentType.Cash).ToString("D2");
        var creditCode = ((int)FiscalPaymentType.CreditCard).ToString("D2");
        var debitCode = ((int)FiscalPaymentType.DebitCard).ToString("D2");

        var byCode = await (
                from payment in _context.SalePayments.AsNoTracking()
                join sale in _context.Sales.AsNoTracking() on payment.SaleId equals sale.Id
                join method in _context.Payments.AsNoTracking() on payment.PaymentId equals method.Id
                where sale.CashShiftId == cashShiftId
                    && (method.PaymentType == cashCode
                        || method.PaymentType == creditCode
                        || method.PaymentType == debitCode)
                group payment by method.PaymentType into g
                select new { Code = g.Key, Total = g.Sum(p => p.Amount) })
            .ToDictionaryAsync(x => x.Code, x => x.Total, ct);

        return new ShiftSalesTotals(
            totalSales,
            byCode.GetValueOrDefault(cashCode),
            byCode.GetValueOrDefault(creditCode),
            byCode.GetValueOrDefault(debitCode));
    }
}
