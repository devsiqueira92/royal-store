using RoyalStore.Domain.Identity;

namespace RoyalStore.Application.Abstractions.Data;

public interface IPasswordResetRepository
{
    Task<int> CountRecentAsync(string userId, Guid tenantId, DateTimeOffset since, CancellationToken ct);
    Task AddAsync(PasswordResetRequest request, CancellationToken ct);
    Task<PasswordResetRequest?> FindActiveByUserIdAsync(string userId, Guid tenantId, CancellationToken ct);
}
