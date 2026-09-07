using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface IOriginOptionRepository
{
    Task AddAsync(OriginOption option, CancellationToken ct = default);
    Task<OriginOption?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<OriginOption>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(OriginOption option, CancellationToken ct = default);
}
