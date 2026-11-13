using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Expenses;

/// <summary>
/// One settlement against one expense, recorded independently of the expense so a bill can be
/// settled over several payments (FR-012).
/// </summary>
/// <remarks>
/// Created only through <see cref="Expense.RegisterPayment"/>, which owns every rule. Reversal is a
/// flag rather than a delete so the settlement history stays auditable.
/// </remarks>
public sealed class ExpensePayment : Entity, IAuditableEntity
{
    public const int MethodNoteMaxLength = 100;
    public const int NotesMaxLength = 500;
    public const int ReversalReasonMaxLength = 500;

    public Guid TenantId { get; private set; }
    public Guid ExpenseId { get; private set; }
    public Guid FinancialAccountId { get; private set; }
    public decimal Amount { get; private set; }
    public DateOnly PaidAt { get; private set; }
    public string? MethodNote { get; private set; }
    public string? Notes { get; private set; }

    public bool IsReversed { get; private set; }
    public DateTimeOffset? ReversedAt { get; private set; }
    public string? ReversalReason { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ExpensePayment() : base(Guid.NewGuid()) { }

    private ExpensePayment(
        Guid id, Guid tenantId, Guid expenseId, Guid financialAccountId,
        decimal amount, DateOnly paidAt, string? methodNote, string? notes)
        : base(id)
    {
        TenantId = tenantId;
        ExpenseId = expenseId;
        FinancialAccountId = financialAccountId;
        Amount = amount;
        PaidAt = paidAt;
        MethodNote = methodNote;
        Notes = notes;
    }

    /// <summary>
    /// Internal so the aggregate root is the only way in: every balance and status rule lives on
    /// <see cref="Expense"/>, and a payment created around it would bypass all of them.
    /// </summary>
    internal static Result<ExpensePayment> Create(
        Guid tenantId, Guid expenseId, Guid financialAccountId,
        decimal amount, DateOnly paidAt, string? methodNote, string? notes)
    {
        if (financialAccountId == Guid.Empty)
            return Error.Validation("expensepayment.account_required",
                "Financial account is required to register a payment.");

        if (!string.IsNullOrWhiteSpace(methodNote))
        {
            methodNote = methodNote.Trim();
            if (methodNote.Length > MethodNoteMaxLength)
                return Error.Validation("expensepayment.method_note_too_long",
                    $"Payment method note must not exceed {MethodNoteMaxLength} characters.");
        }
        else
        {
            methodNote = null;
        }

        if (!string.IsNullOrWhiteSpace(notes))
        {
            notes = notes.Trim();
            if (notes.Length > NotesMaxLength)
                return Error.Validation("expensepayment.notes_too_long",
                    $"Payment notes must not exceed {NotesMaxLength} characters.");
        }
        else
        {
            notes = null;
        }

        return new ExpensePayment(
            Guid.NewGuid(), tenantId, expenseId, financialAccountId, amount, paidAt, methodNote, notes);
    }

    internal Result Reverse(DateTimeOffset reversedAt, string? reason)
    {
        if (IsReversed)
            return Result.Failure(Error.Conflict("expensepayment.already_reversed",
                "This payment has already been reversed."));

        if (!string.IsNullOrWhiteSpace(reason))
        {
            reason = reason.Trim();
            if (reason.Length > ReversalReasonMaxLength)
                return Result.Failure(Error.Validation("expensepayment.reversal_reason_too_long",
                    $"Reversal reason must not exceed {ReversalReasonMaxLength} characters."));
        }
        else
        {
            reason = null;
        }

        IsReversed = true;
        ReversedAt = reversedAt;
        ReversalReason = reason;

        return Result.Success();
    }
}
