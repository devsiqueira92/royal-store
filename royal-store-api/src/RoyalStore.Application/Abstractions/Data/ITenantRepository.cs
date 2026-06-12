using RoyalStore.Domain.Tenants;

namespace RoyalStore.Application.Abstractions.Data;

public interface ITenantRepository
{
    Task<bool> ExistsAsync(string document, CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    Task<Tenant?> FindByIdAsync(Guid id, CancellationToken ct = default);
}
