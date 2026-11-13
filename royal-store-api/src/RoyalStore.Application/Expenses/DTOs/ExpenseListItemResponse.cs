using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>One row of the competence-month list (FR-042), ordered by due date.</summary>
public record ExpenseListItemResponse(
    Guid Id,
    DateOnly DueDate,
    string Description,
    string? PayeeName,
    ExpenseCategorySummary Category,
    decimal Amount,
    decimal SettledAmount,
    decimal RemainingBalance,
    string Status,
    bool IsOverdue,
    int DaysOverdue,
    bool IsEstimated,
    bool IsRecurring)
{
    public static ExpenseListItemResponse From(
        Expense expense, ExpenseCategorySummary category, DateOnly today) =>
        new(expense.Id,
            expense.DueDate,
            expense.Description,
            expense.PayeeName,
            category,
            expense.Amount,
            expense.SettledAmount,
            expense.RemainingBalance,
            ExpenseResponse.ToStatusName(expense.Status),
            expense.IsOverdue(today),
            expense.DaysOverdue(today),
            expense.IsEstimated,
            expense.IsRecurring);
}
