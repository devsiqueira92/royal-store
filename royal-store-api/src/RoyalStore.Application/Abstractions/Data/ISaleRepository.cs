using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Abstractions.Data;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken ct = default);
    Task AddItemsAsync(IEnumerable<SaleItem> items, CancellationToken ct = default);
    Task AddPaymentsAsync(IEnumerable<SalePayment> payments, CancellationToken ct = default);
    Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Sale sale, CancellationToken ct = default);
    Task<ShiftSalesTotals> GetShiftSalesTotalsAsync(Guid cashShiftId, CancellationToken ct = default);

    /// <summary>Sales totals of every shift of the current tenant, keyed by shift id, using the same
    /// classification as <see cref="GetShiftSalesTotalsAsync"/>. Shifts with no sales are absent.</summary>
    Task<IReadOnlyDictionary<Guid, ShiftSalesTotals>> GetSalesTotalsByShiftAsync(CancellationToken ct = default);
}
