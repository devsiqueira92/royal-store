using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// One expense in full, including its settlement state. Returned by create, update, cancel, both
/// payment operations and the detail query, so a caller always sees the recalculated balance and
/// status in the same round trip as the change.
/// </summary>
public record ExpenseResponse(
    Guid Id,
    string Description,
    string? PayeeName,
    Guid? SupplierId,
    ExpenseCategorySummary Category,
    Guid? CostCenterId,
    Guid? FinancialAccountId,
    decimal Amount,
    decimal SettledAmount,
    decimal RemainingBalance,
    bool IsEstimated,
    DateOnly CompetenceMonth,
    DateOnly DueDate,
    string Status,
    bool IsOverdue,
    int DaysOverdue,
    bool IsRecurring,
    Guid? ExpenseRecurrenceId,
    string? Notes,
    IReadOnlyList<ExpensePaymentResponse> Payments,
    IReadOnlyList<ExpenseAttachmentResponse> Attachments)
{
    /// <summary>
    /// Shapes the loaded aggregate. <paramref name="today"/> is passed in rather than read here
    /// because overdue is derived at read time (FR-020) and the caller owns the clock.
    /// </summary>
    public static ExpenseResponse From(Expense expense, ExpenseCategorySummary category, DateOnly today) =>
        new(expense.Id,
            expense.Description,
            expense.PayeeName,
            expense.SupplierId,
            category,
            expense.CostCenterId,
            expense.FinancialAccountId,
            expense.Amount,
            expense.SettledAmount,
            expense.RemainingBalance,
            expense.IsEstimated,
            expense.CompetenceMonth,
            expense.DueDate,
            ToStatusName(expense.Status),
            expense.IsOverdue(today),
            expense.DaysOverdue(today),
            expense.IsRecurring,
            expense.ExpenseRecurrenceId,
            expense.Notes,
            [.. expense.Payments.Select(p => ExpensePaymentResponse.From(p))],
            [.. expense.Attachments.Select(ExpenseAttachmentResponse.From)]);

    public static string ToStatusName(ExpenseStatus status) => status switch
    {
        ExpenseStatus.Pending => "pending",
        ExpenseStatus.PartiallyPaid => "partially-paid",
        ExpenseStatus.Paid => "paid",
        ExpenseStatus.Cancelled => "cancelled",
        _ => status.ToString().ToLowerInvariant(),
    };
}
