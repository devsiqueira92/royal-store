namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>
/// The outcome of one generation run (FR-039). <see cref="CyclesSkipped"/> is what makes a repeat
/// run legible: zero created and N skipped means the bills already existed, not that nothing
/// happened (FR-032).
/// </summary>
public record RecurrenceGenerationResultResponse(
    int TemplatesEvaluated,
    int ExpensesCreated,
    int CyclesSkipped,
    DateOnly? GeneratedThrough,
    DateTimeOffset RunAt);
