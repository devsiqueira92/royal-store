using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Customers;

/// <summary>
/// A person identified at checkout. Tenant-scoped. Managed exclusively by Admin/Manager (FR-005);
/// sellers may read but not mutate (FR-006). Every customer belongs to exactly one
/// <see cref="Domain.Loyalty.LoyaltyTier"/>, defaulting to the tenant's Standard tier when none is
/// chosen (FR-013), and owns a cashback history read through <c>ICashbackLedger</c>.
/// </summary>
public sealed class Customer : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public bool TermsAccepted { get; private set; }
    public Guid LoyaltyTierId { get; private set; }
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Customer() : base(Guid.NewGuid()) { }

    private Customer(Guid id, Guid tenantId, string name, string document, string? phone,
        bool termsAccepted, Guid loyaltyTierId)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Document = document;
        Phone = phone;
        TermsAccepted = termsAccepted;
        LoyaltyTierId = loyaltyTierId;
        IsActive = true;
    }

    public static Result<Customer> Create(Guid tenantId, string name, string document,
        string? phone, bool termsAccepted, Guid loyaltyTierId)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("customer.tenant_required", "Tenant identifier is required.");

        var validation = ValidateNameAndPhone(name, phone);
        if (validation.IsFailure)
            return validation.Error;

        if (string.IsNullOrWhiteSpace(document))
            return Error.Validation("customer.document_required", "Document is required.");

        if (!DocumentValidator.IsValid(document))
            return Error.Validation("customer.document_invalid", "Document must be a valid CPF or CNPJ.");

        if (loyaltyTierId == Guid.Empty)
            return Error.Validation("customer.loyalty_tier_required", "Loyalty tier is required.");

        return new Customer(Guid.NewGuid(), tenantId, name.Trim(),
            DocumentValidator.Normalize(document), NormalizePhone(phone), termsAccepted, loyaltyTierId);
    }

    public Result Update(string name, string? phone)
    {
        var validation = ValidateNameAndPhone(name, phone);
        if (validation.IsFailure)
            return validation;

        Name = name.Trim();
        Phone = NormalizePhone(phone);
        return Result.Success();
    }

    public Result AcceptTerms()
    {
        TermsAccepted = true;
        return Result.Success();
    }

    /// <summary>Reassigns the customer's tier. Existence of <paramref name="loyaltyTierId"/> is the
    /// caller's responsibility to verify against the repository (FR-013) — mirrors how other entities
    /// in this codebase validate foreign references at the handler level, not inside the entity.</summary>
    public Result AssignTier(Guid loyaltyTierId)
    {
        if (loyaltyTierId == Guid.Empty)
            return Result.Failure(Error.Validation("customer.loyalty_tier_required", "Loyalty tier is required."));

        LoyaltyTierId = loyaltyTierId;
        return Result.Success();
    }

    /// <summary>Removal per FR-007: a deactivation, not an erasure — cashback history and past sales
    /// stay attributable. The customer simply stops appearing in listings and stops being selectable
    /// at checkout.</summary>
    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Validation("customer.already_inactive", "Customer is already inactive."));

        IsActive = false;
        return Result.Success();
    }

    public Result Reactivate()
    {
        if (IsActive)
            return Result.Failure(Error.Validation("customer.already_active", "Customer is already active."));

        IsActive = true;
        return Result.Success();
    }

    private static Result ValidateNameAndPhone(string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("customer.name_required", "Name is required."));

        if (name.Trim().Length > 150)
            return Result.Failure(Error.Validation("customer.name_too_long", "Name must not exceed 150 characters."));

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var normalized = NormalizePhone(phone);
            if (string.IsNullOrEmpty(normalized) || normalized.Length > 20)
                return Result.Failure(Error.Validation("customer.phone_invalid",
                    "Phone must contain digits only and must not exceed 20 characters."));
        }

        return Result.Success();
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(digits) ? null : digits;
    }
}
