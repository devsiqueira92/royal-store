using RoyalStore.Domain.Const;

namespace RoyalStore.Domain.Permissions;

/// <summary>
/// The baseline permission set every holder of a role receives automatically,
/// with no individual grant required.
/// </summary>
public static class RolePermissions
{
    private static readonly IReadOnlySet<Permission> AllPermissions =
        PermissionCatalog.All.Select(e => e.Permission).ToHashSet();

    private static readonly IReadOnlySet<Permission> SellerBaseline = new HashSet<Permission>
    {
        Permission.SalesCreate,
        Permission.SalesCancel,
        Permission.SalesRefund,
        Permission.SalesApplyDiscount,
        Permission.SalesOverridePrice,
    };

    private static readonly IReadOnlySet<Permission> Empty = new HashSet<Permission>();

    public static IReadOnlySet<Permission> ForRole(string? role) => role switch
    {
        Role.Admin => AllPermissions,
        Role.Seller => SellerBaseline,
        _ => Empty,
    };
}
