using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Identity;

public sealed class PasswordResetRequest : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public bool IsConsumed { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private PasswordResetRequest() { }

    private PasswordResetRequest(string userId, Guid tenantId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        IsConsumed = false;
    }

    public static Result<PasswordResetRequest> Create(string userId, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Error.Validation("password_reset.user_required", "UserId is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("password_reset.tenant_required", "TenantId must not be empty.");

        return new PasswordResetRequest(userId, tenantId);
    }

    public Result Consume()
    {
        if (IsConsumed)
            return Result.Failure(Error.Validation("password_reset.already_consumed",
                "This password reset request has already been used."));

        IsConsumed = true;
        return Result.Success();
    }
}
