namespace RoyalStore.Domain.Permissions;

/// <summary>
/// The fixed, system-wide catalogue of permissions. Identical for every tenant;
/// never varies at runtime.
/// </summary>
public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionEntry> All { get; } =
    [
        new(Permission.SalesCreate, "sales.create", PermissionResource.Sales, "Create sale"),
        new(Permission.SalesCancel, "sales.cancel", PermissionResource.Sales, "Cancel sale"),
        new(Permission.SalesRefund, "sales.refund", PermissionResource.Sales, "Refund sale"),
        new(Permission.SalesApplyDiscount, "sales.applyDiscount", PermissionResource.Sales, "Apply discount"),
        new(Permission.SalesOverridePrice, "sales.overridePrice", PermissionResource.Sales, "Override price"),

        new(Permission.CashRegisterOpen, "cashRegister.open", PermissionResource.CashRegister, "Open register"),
        new(Permission.CashRegisterClose, "cashRegister.close", PermissionResource.CashRegister, "Close register"),
        new(Permission.CashRegisterCashDrop, "cashRegister.cashDrop", PermissionResource.CashRegister, "Cash drop"),

        new(Permission.InventoryView, "inventory.view", PermissionResource.Inventory, "View inventory"),
        new(Permission.InventoryAdjust, "inventory.adjust", PermissionResource.Inventory, "Adjust inventory"),
        new(Permission.InventoryReceive, "inventory.receive", PermissionResource.Inventory, "Receive stock"),
        new(Permission.InventoryViewCost, "inventory.viewCost", PermissionResource.Inventory, "View cost"),

        new(Permission.CustomersManageCredit, "customers.manageCredit", PermissionResource.Customers, "Manage customer credit"),
        new(Permission.CustomersCreate, "customers.create", PermissionResource.Customers, "Create customer"),

        new(Permission.ReportsViewFinancials, "reports.viewFinancials", PermissionResource.Reports, "View financial reports"),

        new(Permission.UsersManage, "users.manage", PermissionResource.Users, "Manage users"),

        new(Permission.RolesManage, "roles.manage", PermissionResource.Roles, "Manage roles"),

        new(Permission.SystemManagerOverride, "system.managerOverride", PermissionResource.System, "Manager override"),
    ];

    private static readonly Dictionary<Permission, PermissionEntry> ByPermission =
        All.ToDictionary(e => e.Permission);

    private static readonly Dictionary<string, Permission> ByName =
        All.ToDictionary(e => e.Name, e => e.Permission, StringComparer.OrdinalIgnoreCase);

    public static bool TryParse(string? name, out Permission permission)
    {
        if (!string.IsNullOrWhiteSpace(name) &&
            ByName.TryGetValue(name.Trim(), out permission))
        {
            return true;
        }

        permission = default;
        return false;
    }

    public static string NameOf(Permission permission) => ByPermission[permission].Name;

    public static PermissionResource ResourceOf(Permission permission) => ByPermission[permission].Resource;

    public static string LabelOf(Permission permission) => ByPermission[permission].Label;
}
