using RoyalStore.Domain.Permissions;

namespace RoyalStore.Application.Abstractions.Data;

public interface IPermissionGrantRepository
{
    Task<IReadOnlyList<Permission>> GetPermissionsByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Full-replace the user's individual grants with <paramref name="permissions"/>.
    /// Does not call SaveChanges — the caller owns the unit of work.
    /// </summary>
    Task ReplaceForUserAsync(
        string userId,
        Guid tenantId,
        IReadOnlyCollection<Permission> permissions,
        CancellationToken ct = default);
}
