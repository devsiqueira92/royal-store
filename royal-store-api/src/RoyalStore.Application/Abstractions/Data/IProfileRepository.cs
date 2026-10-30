using RoyalStore.Domain.Profiles;

namespace RoyalStore.Application.Abstractions.Data;

public interface IProfileRepository
{
    Task AddAsync(Profile profile, CancellationToken ct = default);
    Task<Profile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Profile?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, Profile>> GetByUserIdsAsync(IEnumerable<string> userIds, CancellationToken ct = default);
    Task<bool> ExistsByCpfAsync(string cpf, Guid? excludeId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(Profile profile, CancellationToken ct = default);
}
