using RoyalStore.Domain.Permissions;

namespace RoyalStore.Domain.Const;

public static class PolicyNames
{
    public const string AdminOrManager = "AdminOrManager";
    public const string Seller = "Seller";
    public const string Admin = "Admin";
    public const string SellerOnboarding = "SellerOnboarding";
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>Admin OR Manager OR an onboarded Seller — read access shared by all three roles
    /// (research.md Decision 7). No existing policy expressed this three-way OR before this feature.</summary>
    public const string StaffRead = "StaffRead";

    /// <summary>Name of the authorization policy that requires a single catalogue permission.</summary>
    public const string PermissionPrefix = "perm:";

    public static string Permission(Permissions.Permission permission) =>
        PermissionPrefix + PermissionCatalog.NameOf(permission);
}
