using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sellers;

public sealed class Seller : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Seller() : base(Guid.NewGuid()) { }

    private Seller(Guid id, Guid tenantId, string fullName, string phone, string cpf, string? userId)
        : base(id)
    {
        TenantId = tenantId;
        FullName = fullName;
        Phone = phone;
        Cpf = cpf;
        IsActive = true;
        UserId = userId;
    }

    public static Result<Seller> Create(Guid tenantId, string fullName, string phone, string cpf, string? userId)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("seller.tenant_required", "Tenant identifier is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            return Error.Validation("seller.full_name_required", "Full name is required.");

        fullName = fullName.Trim();
        if (fullName.Length > 150)
            return Error.Validation("seller.full_name_too_long", "Full name must not exceed 150 characters.");

        if (string.IsNullOrWhiteSpace(phone))
            return Error.Validation("seller.phone_required", "Phone is required.");

        phone = phone.Trim();
        if (phone.Length > 20)
            return Error.Validation("seller.phone_too_long", "Phone must not exceed 20 characters.");

        var normalizedCpf = CpfValidator.Normalize(cpf ?? string.Empty);
        if (!CpfValidator.IsValid(normalizedCpf))
            return Error.Validation("seller.invalid_cpf", "CPF is invalid.");

        return new Seller(Guid.NewGuid(), tenantId, fullName, phone, normalizedCpf, userId);
    }

    public Result Update(string fullName, string phone, string? userId)
    {
        if (!IsActive)
            return Result.Failure(Error.Validation("seller.inactive", "Cannot update an inactive seller."));

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

        FullName = fullName;
        Phone = phone;
        UserId = userId;
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
