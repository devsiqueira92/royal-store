using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;
using RoyalStore.Domain.Sellers;

namespace RoyalStore.Domain.Profiles;

public sealed class Profile : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Cpf { get; private set; }
    public string? Email { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public bool IsComplete => CompletedAt is not null;

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Profile() : base(Guid.NewGuid()) { }

    private Profile(Guid id, Guid tenantId, string fullName, string userId)
        : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        UserId = userId;
    }

    public static Result<Profile> CreateForOnboarding(Guid tenantId, string fullName, string userId)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("profile.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            return Error.Validation("profile.full_name_required", "Full name is required.");

        fullName = fullName.Trim();
        if (fullName.Length > 150)
            return Error.Validation("profile.full_name_too_long", "Full name must not exceed 150 characters.");

        if (string.IsNullOrWhiteSpace(userId))
            return Error.Validation("profile.user_required", "User identifier is required.");

        return new Profile(Guid.NewGuid(), tenantId, fullName, userId);
    }

    public static Result<Profile> CreateOnboarded(Guid tenantId, string fullName, string? phone, string userId)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("profile.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            return Error.Validation("profile.full_name_required", "Full name is required.");

        fullName = fullName.Trim();
        if (fullName.Length > 150)
            return Error.Validation("profile.full_name_too_long", "Full name must not exceed 150 characters.");

        if (!string.IsNullOrWhiteSpace(phone))
        {
            phone = phone.Trim();
            if (phone.Length > 20)
                return Error.Validation("profile.phone_too_long", "Phone must not exceed 20 characters.");
        }
        else
        {
            phone = null;
        }

        if (string.IsNullOrWhiteSpace(userId))
            return Error.Validation("profile.user_required", "User identifier is required.");

        var profile = new Profile(Guid.NewGuid(), tenantId, fullName, userId)
        {
            Phone = phone,
            CompletedAt = DateTimeOffset.UtcNow
        };

        return profile;
    }

    public Result CompleteOnboarding(string fullName, string phone, string? cpf, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(Error.Validation("profile.full_name_required", "Full name is required."));

        fullName = fullName.Trim();
        if (fullName.Length > 150)
            return Result.Failure(Error.Validation("profile.full_name_too_long", "Full name must not exceed 150 characters."));

        if (string.IsNullOrWhiteSpace(phone))
            return Result.Failure(Error.Validation("profile.phone_required", "Phone is required."));

        phone = phone.Trim();
        if (phone.Length > 20)
            return Result.Failure(Error.Validation("profile.phone_too_long", "Phone must not exceed 20 characters."));

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(Error.Validation("profile.email_required", "Email is required."));

        email = email.Trim().ToLowerInvariant();

        // CPF is immutable once set: ignore the incoming value if we already have one.
        if (string.IsNullOrWhiteSpace(Cpf))
        {
            var normalizedCpf = CpfValidator.Normalize(cpf ?? string.Empty);
            if (!CpfValidator.IsValid(normalizedCpf))
                return Result.Failure(Error.Validation("profile.invalid_cpf", "CPF is invalid."));
            Cpf = normalizedCpf;
        }

        FullName = fullName;
        Phone = phone;
        Email = email;

        // Stamp completion once; later profile edits never move or clear it.
        CompletedAt ??= DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result UpdateProfile(string fullName, string? phone)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(Error.Validation("profile.full_name_required", "Full name is required."));

        fullName = fullName.Trim();
        if (fullName.Length > 150)
            return Result.Failure(Error.Validation("profile.full_name_too_long", "Full name must not exceed 150 characters."));

        if (!string.IsNullOrWhiteSpace(phone))
        {
            phone = phone.Trim();
            if (phone.Length > 20)
                return Result.Failure(Error.Validation("profile.phone_too_long", "Phone must not exceed 20 characters."));
        }
        else
        {
            phone = null;
        }

        FullName = fullName;
        Phone = phone;

        // Defensive: this method should only ever run against a row that already exists,
        // but stamp completion if somehow missing so IsComplete never regresses.
        CompletedAt ??= DateTimeOffset.UtcNow;

        return Result.Success();
    }
}
