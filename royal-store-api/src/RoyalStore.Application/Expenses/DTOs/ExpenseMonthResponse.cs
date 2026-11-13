namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>The month view: its four figures plus the page of expenses behind them.</summary>
/// <param name="Summary">
/// Always computed over the WHOLE competence month, never over the filtered page — the reference
/// screens place these cards above the filter tabs, and a figure that moved when a filter changed
/// could not answer "what do I owe this month?".
/// </param>
/// <param name="ListedTotal">
/// The sum of <paramref name="Items"/>, so a filtered list still has a total that re-sums. Equal to
/// <c>Summary.Total</c> when no filter is applied.
/// </param>
public record ExpenseMonthResponse(
    string CompetenceMonth,
    ExpenseMonthSummaryResponse Summary,
    IReadOnlyList<ExpenseListItemResponse> Items,
    decimal ListedTotal,
    int Page,
    int PageSize,
    int TotalCount);
