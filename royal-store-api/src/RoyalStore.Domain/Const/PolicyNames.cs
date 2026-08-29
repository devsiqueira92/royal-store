using RoyalStore.Domain.Permissions;

namespace RoyalStore.Domain.Const;

public static class PolicyNames
{
    public const string AdminOrManager = "AdminOrManager";
    public const string Seller = "Seller";
    public const string Admin = "Admin";
    public const string SellerOnboarding = "SellerOnboarding";

    /// <summary>Name of the authorization policy that requires a single catalogue permission.</summary>
    public const string PermissionPrefix = "perm:";

    public static string Permission(Permissions.Permission permission) =>
        PermissionPrefix + PermissionCatalog.NameOf(permission);
}
