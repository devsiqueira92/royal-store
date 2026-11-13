using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// One settlement. A reversed payment is still returned, flagged — reversal is a flag rather than a
/// delete so the settlement history stays auditable.
/// </summary>
public record ExpensePaymentResponse(
    Guid Id,
    Guid ExpenseId,
    Guid FinancialAccountId,
    string? FinancialAccountName,
    decimal Amount,
    DateOnly PaidAt,
    string? MethodNote,
    string? Notes,
    bool IsReversed,
    DateTimeOffset? ReversedAt,
    string? ReversalReason,
    string RegisteredBy,
    DateTimeOffset RegisteredAt)
{
    public static ExpensePaymentResponse From(ExpensePayment payment, string? financialAccountName = null) =>
        new(payment.Id,
            payment.ExpenseId,
            payment.FinancialAccountId,
            financialAccountName,
            payment.Amount,
            payment.PaidAt,
            payment.MethodNote,
            payment.Notes,
            payment.IsReversed,
            payment.ReversedAt,
            payment.ReversalReason,
            payment.CreatedBy,
            payment.CreatedAt);
}
