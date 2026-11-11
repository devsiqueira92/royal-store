using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.Queries.GetSalesList;
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

    // Seller name resolved with the same profile-first, login-name-fallback rule
    // CashShiftRepository.GetAllWithMovementTotalsAsync already uses for "which store user did this" —
    // both lookups are already-null-safe against a null UserId (057 research Decision 2).
    public async Task<IReadOnlyList<SaleWithNamesProjection>> GetAllAsync(
        string? normalizedSellerName = null,
        string? normalizedCustomerName = null,
        DateOnly? soldFrom = null,
        DateOnly? soldTo = null,
        FulfillmentMode? fulfillmentMode = null,
        CancellationToken ct = default)
    {
        var query = _context.Sales.AsNoTracking().AsQueryable();

        // Day-range bounds on SoldAt, inclusive on both ends (057 research Decision 4). SoldAt is
        // stored as "timestamp with time zone"; Npgsql requires Kind=Utc, never Unspecified (the
        // Kind DateOnly.ToDateTime produces by default).
        if (soldFrom.HasValue)
        {
            var from = DateTime.SpecifyKind(soldFrom.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            query = query.Where(s => s.SoldAt >= from);
        }

        if (soldTo.HasValue)
        {
            var to = DateTime.SpecifyKind(soldTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            query = query.Where(s => s.SoldAt < to);
        }

        if (fulfillmentMode.HasValue)
            query = query.Where(s => s.FulfillmentMode == fulfillmentMode.Value);

        // Accent-/case-insensitive match via the existing fn_normalize_search_text function, mapped
        // as a DbFunction rather than a new computed column (057 research Decision 3).
        if (!string.IsNullOrEmpty(normalizedCustomerName))
            query = query.Where(s => s.CustomerSnapshot != null
                && RoyalStoreContext.NormalizeSearchText(s.CustomerSnapshot.Name).Contains(normalizedCustomerName));

        // Filtered directly against Sale (the same correlated sub-select used again in the final
        // Select below) rather than against the projected SaleWithNamesProjection record: EF cannot
        // translate a Where that reads SellerName back off a freshly constructed record — it throws
        // "could not be translated" at runtime despite compiling, because the record's SellerName
        // field is itself a correlated sub-select, not a plain column.
        if (!string.IsNullOrEmpty(normalizedSellerName))
            query = query.Where(s => RoyalStoreContext.NormalizeSearchText(
                    _context.Profiles.Where(p => p.UserId == s.UserId).Select(p => p.FullName).FirstOrDefault()
                        ?? _context.Users.Where(u => u.Id == s.UserId).Select(u => u.UserName).FirstOrDefault())
                .Contains(normalizedSellerName));

        // Order on the entity's own column before projecting — Select/Where preserve ordering, but
        // EF cannot translate OrderByDescending(p => p.SoldAt) once SoldAt is read back off a freshly
        // constructed SaleWithNamesProjection, same issue as the seller-name filter above.
        return await query
            .OrderByDescending(s => s.SoldAt)
            .Select(s => new SaleWithNamesProjection(
                s.Id,
                _context.Profiles.Where(p => p.UserId == s.UserId).Select(p => p.FullName).FirstOrDefault()
                    ?? _context.Users.Where(u => u.Id == s.UserId).Select(u => u.UserName).FirstOrDefault(),
                s.Subtotal,
                s.DiscountAmount,
                s.CustomerSnapshot != null ? s.CustomerSnapshot.Name : null,
                s.SoldAt,
                s.FulfillmentMode))
            .ToListAsync(ct);
    }
}
