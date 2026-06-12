using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Tenants;

public class Tenant : Entity, IAuditableEntity
{
    public string StoreName { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public DocumentType DocumentType { get; private set; }
    public TenantStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    protected Tenant() : base(Guid.NewGuid()) { }

    private Tenant(Guid id, string storeName, string document, DocumentType documentType)
        : base(id)
    {
        StoreName = storeName;
        Document = document;
        DocumentType = documentType;
        Status = TenantStatus.Pending;
    }

    public static Result<Tenant> Create(string storeName, string document, DocumentType documentType)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            return Error.Validation("tenant.store_name_required", "Store name is required.");

        storeName = storeName.Trim();

        if (storeName.Length < 2 || storeName.Length > 100)
            return Error.Validation("registration.invalid_store_name", "Store name must be between 2 and 100 characters.");

        if (!Enum.IsDefined(typeof(DocumentType), documentType))
            return Error.Validation("tenant.invalid_document_type", "Document type must be CNPJ or CPF.");

        var normalized = NormalizeDocument(document);

        var expectedLength = documentType == DocumentType.CNPJ ? 14 : 11;
        if (normalized.Length != expectedLength)
            return Error.Validation("registration.invalid_document",
                $"The provided {documentType} must have {expectedLength} digits.");

        if (IsAllSameDigits(normalized))
            return Error.Validation("registration.invalid_document",
                $"The provided {documentType} is invalid.");

        var valid = documentType == DocumentType.CNPJ
            ? IsValidCnpj(normalized)
            : IsValidCpf(normalized);

        if (!valid)
            return Error.Validation("registration.invalid_document",
                $"The provided {documentType} is invalid.");

        var tenant = new Tenant(Guid.NewGuid(), storeName, normalized, documentType);
        tenant.RaiseDomainEvent(new TenantRegisteredDomainEvent(tenant.Id));
        return tenant;
    }

    public void Activate() => Status = TenantStatus.Active;

    private static string NormalizeDocument(string document) =>
        new string(document.Where(char.IsDigit).ToArray());

    private static bool IsAllSameDigits(string digits) =>
        digits.Distinct().Count() == 1;

    private static bool IsValidCnpj(string cnpj)
    {
        // First check digit — weights: 5,4,3,2,9,8,7,6,5,4,3,2
        int[] weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        var sum = 0;
        for (var i = 0; i < 12; i++)
            sum += (cnpj[i] - '0') * weights1[i];

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if (digit1 != cnpj[12] - '0')
            return false;

        // Second check digit — weights: 6,5,4,3,2,9,8,7,6,5,4,3,2
        int[] weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        sum = 0;
        for (var i = 0; i < 13; i++)
            sum += (cnpj[i] - '0') * weights2[i];

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return digit2 == cnpj[13] - '0';
    }

    private static bool IsValidCpf(string cpf)
    {
        // First check digit — weights: 10,9,8,7,6,5,4,3,2
        var sum = 0;
        for (var i = 0; i < 9; i++)
            sum += (cpf[i] - '0') * (10 - i);

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if (digit1 != cpf[9] - '0')
            return false;

        // Second check digit — weights: 11,10,9,8,7,6,5,4,3,2
        sum = 0;
        for (var i = 0; i < 10; i++)
            sum += (cpf[i] - '0') * (11 - i);

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return digit2 == cpf[10] - '0';
    }
}
