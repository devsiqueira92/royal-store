using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class PasswordResetRepository(RoyalStoreContext context) : IPasswordResetRepository
{
    public Task<int> CountRecentAsync(
        string userId,
        Guid tenantId,
        DateTimeOffset since,
        CancellationToken ct)
        => context.PasswordResetRequests
            .IgnoreQueryFilters()
            .CountAsync(r =>
                r.UserId == userId &&
                r.TenantId == tenantId &&
                !r.IsConsumed &&
                r.CreatedAt >= since, ct);

    public async Task AddAsync(PasswordResetRequest request, CancellationToken ct)
        => await context.PasswordResetRequests.AddAsync(request, ct);

    public Task<PasswordResetRequest?> FindActiveByUserIdAsync(
        string userId,
        Guid tenantId,
        CancellationToken ct)
        => context.PasswordResetRequests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.TenantId == tenantId &&
                !r.IsConsumed, ct);
}
