using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Tenants;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private readonly RoyalStoreContext _context;

    public TenantRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(string document, CancellationToken ct = default) =>
        _context.Tenants.AnyAsync(t => t.Document == document, ct);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) =>
        await _context.Tenants.AddAsync(tenant, ct);

    public Task<Tenant?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
}
