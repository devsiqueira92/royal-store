namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// The four headline figures for a competence month (FR-041). They always reconcile with the listed
/// expenses: cancelled ones are excluded entirely (FR-008), and <see cref="Overdue"/> counts the
/// unsettled balance rather than the full amount (FR-021), so a partially paid overdue bill
/// contributes only what is still owed.
/// </summary>
public record ExpenseMonthSummaryResponse(
    decimal Total,
    int ExpenseCount,
    decimal Paid,
    int PaidPercentage,
    decimal Pending,
    int PendingCount,
    decimal Overdue,
    int OverdueCount);
