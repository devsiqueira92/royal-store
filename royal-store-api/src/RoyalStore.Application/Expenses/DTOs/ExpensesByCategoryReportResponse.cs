namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// One category's row in the management P&amp;L (DRE gerencial).
/// </summary>
/// <param name="Values">
/// One entry per month, in the same order as the report's month list. A null means NO expense was
/// recorded that month — rendered as a dash, not a zero, because "nothing recorded" and "recorded
/// as zero" are different facts (FR-044).
/// </param>
/// <param name="Share">This category's percentage of the last month in the range.</param>
public record ExpenseCategoryReportRow(
    Guid CategoryId,
    string Name,
    string? Color,
    IReadOnlyList<decimal?> Values,
    decimal Share);

/// <summary>
/// Expenses by category over a run of competence months (FR-044). A parent category's figures
/// include every descendant's (FR-023).
/// </summary>
public record ExpensesByCategoryReportResponse(
    IReadOnlyList<string> Months,
    IReadOnlyList<ExpenseCategoryReportRow> Categories,
    IReadOnlyList<decimal> Totals);
