using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class EmailChangeRequestRepository(RoyalStoreContext context) : IEmailChangeRequestRepository
{
    public Task<int> CountRecentAsync(
        string userId,
        Guid tenantId,
        bool wasSuccessful,
        DateTimeOffset since,
        CancellationToken ct)
        => context.EmailChangeRequests
            .IgnoreQueryFilters()
            .CountAsync(r =>
                r.UserId == userId &&
                r.TenantId == tenantId &&
                r.WasSuccessful == wasSuccessful &&
                r.CreatedAt >= since, ct);

    public async Task AddAsync(EmailChangeRequest request, CancellationToken ct)
        => await context.EmailChangeRequests.AddAsync(request, ct);
}
