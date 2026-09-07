using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Abstractions.Data;

public interface ICashShiftRepository
{
    Task AddAsync(CashShift cashShift, CancellationToken ct = default);
    Task<CashShift?> GetOpenByUserIdAsync(string userId, CancellationToken ct = default);
    Task<CashShift?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(CashShift cashShift, CancellationToken ct = default);
    Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
