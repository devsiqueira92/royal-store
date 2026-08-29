using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Permissions;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class PermissionGrantRepository : IPermissionGrantRepository
{
    private readonly RoyalStoreContext _context;

    public PermissionGrantRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    // UserId is a globally unique FK to AspNetUsers.Id, so ignoring the tenant
    // query filter is safe and lets anonymous login resolve a user's grants.
    public async Task<IReadOnlyList<Permission>> GetPermissionsByUserIdAsync(
        string userId, CancellationToken ct = default) =>
        await _context.UserPermissionGrants
            .IgnoreQueryFilters()
            .Where(g => g.UserId == userId)
            .Select(g => g.Permission)
            .ToListAsync(ct);

    public async Task ReplaceForUserAsync(
        string userId,
        Guid tenantId,
        IReadOnlyCollection<Permission> permissions,
        CancellationToken ct = default)
    {
        var existing = await _context.UserPermissionGrants
            .Where(g => g.UserId == userId)
            .ToListAsync(ct);

        _context.UserPermissionGrants.RemoveRange(existing);

        foreach (var permission in permissions.Distinct())
        {
            var grant = UserPermissionGrant.Create(tenantId, userId, permission);
            if (grant.IsSuccess)
                await _context.UserPermissionGrants.AddAsync(grant.Value, ct);
        }
    }
}
