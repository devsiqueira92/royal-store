using RoyalStore.Domain.Identity;

namespace RoyalStore.Application.Abstractions.Data;

public interface IConfirmationResendRequestRepository
{
    Task<int> CountRecentAsync(string userId, Guid tenantId, DateTimeOffset since, CancellationToken ct);
    Task AddAsync(ConfirmationResendRequest request, CancellationToken ct);
    Task<ConfirmationResendRequest?> FindActiveByUserIdAsync(string userId, Guid tenantId, CancellationToken ct);
}
