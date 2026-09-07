using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Customers;

/// <summary>
/// A person identified at checkout. Tenant-scoped. The structure exists so that customer identity
/// can later be attached to sales, loyalty and fiscal documents; nothing links a customer to a sale
/// yet — see feature 035, FR-026.
/// </summary>
public sealed class Customer : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public bool TermsAccepted { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Customer() : base(Guid.NewGuid()) { }

    private Customer(Guid id, Guid tenantId, string name, string document, string? phone, bool termsAccepted)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Document = document;
        Phone = phone;
        TermsAccepted = termsAccepted;
    }

    public static Result<Customer> Create(Guid tenantId, string name, string document,
        string? phone, bool termsAccepted)
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

        return new Customer(Guid.NewGuid(), tenantId, name.Trim(),
            DocumentValidator.Normalize(document), NormalizePhone(phone), termsAccepted);
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
