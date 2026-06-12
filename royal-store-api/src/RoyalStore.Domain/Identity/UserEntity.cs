using Microsoft.AspNetCore.Identity;
using RoyalStore.Domain.Common;

namespace RoyalStore.Domain.Identity;

public class UserEntity : IdentityUser
{
    public Guid TenantId { get; private set; }

    protected UserEntity() { }

    private UserEntity(string email, Guid tenantId) : base(email)
    {
        Email = email;
        TenantId = tenantId;
    }

    public static Result<UserEntity> Create(string email, Guid tenantId)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Error.Validation("user.email_required", "Email is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("user.tenant_required", "TenantId must not be empty.");

        return new UserEntity(email, tenantId);
    }
}
