using RoyalStore.Domain.Const;

namespace RoyalStore.Domain.Permissions;

/// <summary>
/// Resolves a user's effective permissions: the union of their role baseline and
/// their individual grants — except Administrators, who always resolve to the
/// full catalogue and whose grants are ignored.
/// </summary>
public static class EffectivePermissions
{
    public static IReadOnlySet<Permission> For(string? role, IEnumerable<Permission> individualGrants)
    {
        if (role == Role.Admin)
            return RolePermissions.ForRole(Role.Admin);

        var effective = new HashSet<Permission>(RolePermissions.ForRole(role));
        effective.UnionWith(individualGrants);
        return effective;
    }
}
