using Microsoft.AspNetCore.Identity;
using RoyalStore.Domain.Common;

namespace RoyalStore.Domain.Identity;

public class UserEntity : IdentityUser
{
    public Guid TenantId { get; private set; }
    public bool RequiresPasswordChange { get; private set; }

    protected UserEntity() { }

    private UserEntity(string email, Guid tenantId) : base(email)
    {
        Email = email;
        TenantId = tenantId;
    }

    private UserEntity(string userName, Guid tenantId, bool requiresPasswordChange) : base()
    {
        UserName = userName;
        TenantId = tenantId;
        RequiresPasswordChange = requiresPasswordChange;
    }

    public static Result<UserEntity> Create(string email, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Error.Validation("user.email_required", "Email is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("user.tenant_required", "TenantId must not be empty.");

        return new UserEntity(email, tenantId);
    }

    public static Result<UserEntity> CreateSeller(string userName, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(userName))
            return Error.Validation("user.username_required", "UserName is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("user.tenant_required", "TenantId must not be empty.");

        return new UserEntity(userName, tenantId, requiresPasswordChange: true);
    }
}
