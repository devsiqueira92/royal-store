using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sellers;

public sealed class Seller : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Cpf { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; }
    public string? UserId { get; private set; }
    public DateTimeOffset? OnboardingCompletedAt { get; private set; }

    public bool IsOnboardingComplete => OnboardingCompletedAt is not null;

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Seller() : base(Guid.NewGuid()) { }

    private Seller(Guid id, Guid tenantId, string fullName, string userId)
        : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        UserId = userId;
        IsActive = true;
    }

    public static Result<Seller> CreateForOnboarding(Guid tenantId, string fullName, string userId)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("seller.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            return Error.Validation("seller.full_name_required", "Full name is required.");

        fullName = fullName.Trim();
        if (fullName.Length > 150)
            return Error.Validation("seller.full_name_too_long", "Full name must not exceed 150 characters.");

        if (string.IsNullOrWhiteSpace(userId))
            return Error.Validation("seller.user_required", "User identifier is required.");

        return new Seller(Guid.NewGuid(), tenantId, fullName, userId);
    }

    public Result CompleteOnboarding(string fullName, string phone, string? cpf, string email)
    {
        if (!IsActive)
            return Result.Failure(Error.Validation("seller.inactive", "Cannot complete onboarding for an inactive seller."));

        if (string.IsNullOrWhiteSpace(fullName))
            return Result.Failure(Error.Validation("seller.full_name_required", "Full name is required."));

        fullName = fullName.Trim();
        if (fullName.Length > 150)
            return Result.Failure(Error.Validation("seller.full_name_too_long", "Full name must not exceed 150 characters."));

        if (string.IsNullOrWhiteSpace(phone))
            return Result.Failure(Error.Validation("seller.phone_required", "Phone is required."));

        phone = phone.Trim();
        if (phone.Length > 20)
            return Result.Failure(Error.Validation("seller.phone_too_long", "Phone must not exceed 20 characters."));

        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure(Error.Validation("seller.email_required", "Email is required."));

        email = email.Trim().ToLowerInvariant();

        // CPF is immutable once set: ignore the incoming value if we already have one.
        if (string.IsNullOrWhiteSpace(Cpf))
        {
            var normalizedCpf = CpfValidator.Normalize(cpf ?? string.Empty);
            if (!CpfValidator.IsValid(normalizedCpf))
                return Result.Failure(Error.Validation("seller.invalid_cpf", "CPF is invalid."));
            Cpf = normalizedCpf;
        }

        FullName = fullName;
        Phone = phone;
        Email = email;

        // Stamp onboarding completion once; later profile edits never move or clear it.
        OnboardingCompletedAt ??= DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("seller.already_deactivated", "Seller is already deactivated."));

        IsActive = false;
        return Result.Success();
    }
}
