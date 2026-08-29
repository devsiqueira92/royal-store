using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Permissions;

/// <summary>
/// An additive association between a user and a single catalogue permission, on
/// top of that user's role baseline. Created or deleted, never edited.
/// </summary>
public sealed class UserPermissionGrant : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public Permission Permission { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private UserPermissionGrant() : base(Guid.NewGuid()) { }

    private UserPermissionGrant(Guid id, Guid tenantId, string userId, Permission permission)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        Permission = permission;
    }

    public static Result<UserPermissionGrant> Create(Guid tenantId, string userId, Permission permission)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("permission.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(userId))
            return Error.Validation("permission.user_required", "User identifier is required.");

        return new UserPermissionGrant(Guid.NewGuid(), tenantId, userId, permission);
    }
}
