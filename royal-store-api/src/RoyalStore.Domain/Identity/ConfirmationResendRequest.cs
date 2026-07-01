using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Identity;

public sealed class ConfirmationResendRequest : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public bool IsConsumed { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ConfirmationResendRequest() { }

    private ConfirmationResendRequest(string userId, Guid tenantId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        IsConsumed = false;
    }

    public static Result<ConfirmationResendRequest> Create(string userId, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Error.Validation("confirmation_resend.user_required", "UserId is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("confirmation_resend.tenant_required", "TenantId must not be empty.");

        return new ConfirmationResendRequest(userId, tenantId);
    }

    public Result Consume()
    {
        if (IsConsumed)
            return Result.Failure(Error.Validation("confirmation_resend.already_consumed",
                "This confirmation resend request has already been superseded."));

        IsConsumed = true;
        return Result.Success();
    }
}
