using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface IUnitMeasureOptionRepository
{
    Task AddAsync(UnitMeasureOption option, CancellationToken ct = default);
    Task<UnitMeasureOption?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<UnitMeasureOption>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(UnitMeasureOption option, CancellationToken ct = default);
}
