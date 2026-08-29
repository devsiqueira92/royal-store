namespace RoyalStore.Application.Permissions.DTOs;

public record PermissionCatalogResponse(IReadOnlyList<PermissionResourceGroup> Resources);

public record PermissionResourceGroup(string Resource, IReadOnlyList<PermissionItem> Permissions);

public record PermissionItem(string Name, string Label);
