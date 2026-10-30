using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sellers;

public sealed class Seller : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public bool IsActive { get; private set; }
    public string? UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Seller() : base(Guid.NewGuid()) { }

    private Seller(Guid id, Guid tenantId, string userId)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        IsActive = true;
    }

    public static Result<Seller> Create(Guid tenantId, string userId)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("seller.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(userId))
            return Error.Validation("seller.user_required", "User identifier is required.");

        return new Seller(Guid.NewGuid(), tenantId, userId);
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("seller.already_deactivated", "Seller is already deactivated."));

        IsActive = false;
        return Result.Success();
    }
}
