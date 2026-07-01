using RoyalStore.Domain.Identity;

namespace RoyalStore.Application.Abstractions.Data;

public interface IEmailChangeRequestRepository
{
    Task<int> CountRecentAsync(string userId, Guid tenantId, bool wasSuccessful, DateTimeOffset since, CancellationToken ct);
    Task AddAsync(EmailChangeRequest request, CancellationToken ct);
}
