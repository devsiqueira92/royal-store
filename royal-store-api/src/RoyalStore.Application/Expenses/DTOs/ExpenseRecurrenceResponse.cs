using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.DTOs;

/// <summary>A recurring template with where it has got to (FR-036).</summary>
/// <param name="NextBillDate">The due date of the next bill, or null once the template is finished.</param>
/// <param name="GeneratesOn">When that bill will be created: <paramref name="NextBillDate"/> minus the lead days.</param>
public record ExpenseRecurrenceResponse(
    Guid Id,
    string Description,
    string? PayeeName,
    Guid? SupplierId,
    ExpenseCategorySummary Category,
    Guid? CostCenterId,
    Guid? FinancialAccountId,
    string Frequency,
    int DueDay,
    int LeadDays,
    decimal Amount,
    bool IsVariableAmount,
    bool IsActive,
    DateOnly StartDate,
    DateOnly? EndDate,
    int? MaxOccurrences,
    int OccurrencesGenerated,
    DateOnly? GeneratedThrough,
    DateOnly? NextBillDate,
    DateOnly? GeneratesOn,
    DateTimeOffset? LastRunAt)
{
    public static ExpenseRecurrenceResponse From(
        ExpenseRecurrence recurrence, ExpenseCategorySummary category)
    {
        var next = recurrence.NextBillDate();

        return new ExpenseRecurrenceResponse(
            recurrence.Id,
            recurrence.Description,
            recurrence.PayeeName,
            recurrence.SupplierId,
            category,
            recurrence.CostCenterId,
            recurrence.FinancialAccountId,
            ToFrequencyName(recurrence.Frequency),
            recurrence.DueDay,
            recurrence.LeadDays,
            recurrence.Amount,
            recurrence.IsVariableAmount,
            recurrence.IsActive,
            recurrence.StartDate,
            recurrence.EndDate,
            recurrence.MaxOccurrences,
            recurrence.OccurrencesGenerated,
            recurrence.GeneratedThrough,
            next,
            next?.AddDays(-recurrence.LeadDays),
            recurrence.LastRunAt);
    }

    public static string ToFrequencyName(RecurrenceFrequency frequency) => frequency switch
    {
        RecurrenceFrequency.Weekly => "weekly",
        RecurrenceFrequency.Monthly => "monthly",
        RecurrenceFrequency.Quarterly => "quarterly",
        RecurrenceFrequency.Semiannually => "semiannually",
        RecurrenceFrequency.Yearly => "yearly",
        _ => frequency.ToString().ToLowerInvariant(),
    };

    /// <summary>
    /// The inverse of <see cref="ToFrequencyName"/>, so a client can send back the very name it was
    /// given. Null means the value is not a frequency — the commands turn that into a validation
    /// error rather than quietly falling back to a default.
    /// </summary>
    public static RecurrenceFrequency? ParseFrequency(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "weekly" => RecurrenceFrequency.Weekly,
            "monthly" => RecurrenceFrequency.Monthly,
            "quarterly" => RecurrenceFrequency.Quarterly,
            "semiannually" => RecurrenceFrequency.Semiannually,
            "yearly" => RecurrenceFrequency.Yearly,
            _ => null,
        };

    /// <summary>The accepted names, for a validation message that lists them.</summary>
    public const string FrequencyNames = "weekly, monthly, quarterly, semiannually, yearly";
}
