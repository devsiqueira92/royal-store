using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Tenants;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository : ITenantRepository
{
    private const string DocumentHash = nameof(Tenant.Document) + "Hash";

    private readonly RoyalStoreContext _context;

    public TenantRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    // The document is encrypted with a random nonce, so it's matched on its blind index.
    public Task<bool> ExistsAsync(string document, CancellationToken ct = default)
    {
        var hash = _context.ComputeBlindIndex<Tenant>(nameof(Tenant.Document), document);
        return _context.Tenants.AnyAsync(t => EF.Property<string?>(t, DocumentHash) == hash, ct);
    }

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) =>
        await _context.Tenants.AddAsync(tenant, ct);

    public Task<Tenant?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
}
