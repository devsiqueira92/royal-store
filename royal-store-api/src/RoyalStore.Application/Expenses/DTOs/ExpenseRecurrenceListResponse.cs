namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// The templates plus what they commit the store to each month. <see cref="MonthlyTotal"/> is the
/// active templates normalised to a monthly figure, so a quarterly bill counts as a third of itself.
/// </summary>
public record ExpenseRecurrenceListResponse(
    IReadOnlyList<ExpenseRecurrenceResponse> Items,
    int ActiveCount,
    int TotalCount,
    decimal MonthlyTotal);
