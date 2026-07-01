using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class ConfirmationResendRequestRepository(RoyalStoreContext context) : IConfirmationResendRequestRepository
{
    public Task<int> CountRecentAsync(
        string userId,
        Guid tenantId,
        DateTimeOffset since,
        CancellationToken ct)
        => context.ConfirmationResendRequests
            .IgnoreQueryFilters()
            .CountAsync(r =>
                r.UserId == userId &&
                r.TenantId == tenantId &&
                r.CreatedAt >= since, ct);

    public async Task AddAsync(ConfirmationResendRequest request, CancellationToken ct)
        => await context.ConfirmationResendRequests.AddAsync(request, ct);

    public Task<ConfirmationResendRequest?> FindActiveByUserIdAsync(
        string userId,
        Guid tenantId,
        CancellationToken ct)
        => context.ConfirmationResendRequests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.TenantId == tenantId &&
                !r.IsConsumed, ct);
}
