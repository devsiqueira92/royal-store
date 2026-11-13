using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// A receipt attached to an expense. Always an empty collection in feature 060 — attachments are
/// modelled but no upload exists yet (FR-050).
/// </summary>
public record ExpenseAttachmentResponse(
    Guid Id,
    Guid ExpenseId,
    string FileName,
    string ContentType,
    long SizeInBytes,
    string AttachedBy,
    DateTimeOffset AttachedAt)
{
    public static ExpenseAttachmentResponse From(ExpenseAttachment attachment) =>
        new(attachment.Id,
            attachment.ExpenseId,
            attachment.FileName,
            attachment.ContentType,
            attachment.SizeInBytes,
            attachment.CreatedBy,
            attachment.CreatedAt);

    // StorageReference is deliberately NOT exposed: it is an internal key whose shape belongs to
    // whatever storage the later feature chooses, and clients must not start depending on it.
}
