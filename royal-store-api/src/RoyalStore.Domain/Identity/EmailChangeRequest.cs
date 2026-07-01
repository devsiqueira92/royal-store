using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Identity;

public sealed class EmailChangeRequest : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public Guid TenantId { get; private set; }
    public bool WasSuccessful { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private EmailChangeRequest() { }

    private EmailChangeRequest(string userId, Guid tenantId, bool wasSuccessful)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        WasSuccessful = wasSuccessful;
    }

    public static Result<EmailChangeRequest> Create(string userId, Guid tenantId, bool wasSuccessful)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Error.Validation("email_change_request.user_required", "UserId is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("email_change_request.tenant_required", "TenantId must not be empty.");

        return new EmailChangeRequest(userId, tenantId, wasSuccessful);
    }
}
