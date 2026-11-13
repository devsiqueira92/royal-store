using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Expenses;

/// <summary>
/// A receipt or supporting document belonging to an expense (FR-049).
/// </summary>
/// <remarks>
/// MODELLED ONLY. This feature implements no upload, download or storage, and nothing creates an
/// attachment yet (FR-050). <see cref="StorageReference"/> is deliberately opaque — a key in
/// whatever store a later feature chooses — so no provider decision is fixed here. The factory
/// exists so that later feature has nothing to change in the domain.
/// </remarks>
public sealed class ExpenseAttachment : Entity, IAuditableEntity
{
    public const int FileNameMaxLength = 260;
    public const int ContentTypeMaxLength = 100;
    public const int StorageReferenceMaxLength = 1024;

    public Guid TenantId { get; private set; }
    public Guid ExpenseId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeInBytes { get; private set; }
    public string StorageReference { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ExpenseAttachment() : base(Guid.NewGuid()) { }

    private ExpenseAttachment(
        Guid id, Guid tenantId, Guid expenseId,
        string fileName, string contentType, long sizeInBytes, string storageReference)
        : base(id)
    {
        TenantId = tenantId;
        ExpenseId = expenseId;
        FileName = fileName;
        ContentType = contentType;
        SizeInBytes = sizeInBytes;
        StorageReference = storageReference;
    }

    public static Result<ExpenseAttachment> Create(
        Guid tenantId, Guid expenseId,
        string fileName, string contentType, long sizeInBytes, string storageReference)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("expenseattachment.tenant_required", "Tenant identifier is required.");

        if (expenseId == Guid.Empty)
            return Error.Validation("expenseattachment.expense_required", "Expense identifier is required.");

        if (string.IsNullOrWhiteSpace(fileName))
            return Error.Validation("expenseattachment.file_name_required", "File name is required.");

        fileName = fileName.Trim();
        if (fileName.Length > FileNameMaxLength)
            return Error.Validation("expenseattachment.file_name_too_long",
                $"File name must not exceed {FileNameMaxLength} characters.");

        if (string.IsNullOrWhiteSpace(contentType))
            return Error.Validation("expenseattachment.content_type_required", "Content type is required.");

        contentType = contentType.Trim();
        if (contentType.Length > ContentTypeMaxLength)
            return Error.Validation("expenseattachment.content_type_too_long",
                $"Content type must not exceed {ContentTypeMaxLength} characters.");

        if (sizeInBytes <= 0)
            return Error.Validation("expenseattachment.size_invalid", "File size must be greater than zero.");

        if (string.IsNullOrWhiteSpace(storageReference))
            return Error.Validation("expenseattachment.storage_reference_required",
                "Storage reference is required.");

        storageReference = storageReference.Trim();
        if (storageReference.Length > StorageReferenceMaxLength)
            return Error.Validation("expenseattachment.storage_reference_too_long",
                $"Storage reference must not exceed {StorageReferenceMaxLength} characters.");

        return new ExpenseAttachment(
            Guid.NewGuid(), tenantId, expenseId, fileName, contentType, sizeInBytes, storageReference);
    }
}
