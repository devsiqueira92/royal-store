namespace RoyalStore.Domain.Permissions;

public sealed record PermissionEntry(
    Permission Permission,
    string Name,
    PermissionResource Resource,
    string Label);
