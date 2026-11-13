using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.CreateExpenseRecurrence;

/// <summary>Registers a recurring expense template (FR-030).</summary>
/// <param name="Frequency">
/// One of the names a response carries — <c>monthly</c>, <c>weekly</c>, … — and NOT the enum.
/// Taking the enum directly meant the API emitted <c>"monthly"</c> but accepted only <c>1</c>, so a
/// client that sent back a value it had just been given got a 400 out of the deserializer, before
/// any validation could explain why.
/// </param>
public record CreateExpenseRecurrenceCommand(
    string Description,
    string? PayeeName,
    Guid? SupplierId,
    Guid ExpenseCategoryId,
    Guid? CostCenterId,
    Guid? FinancialAccountId,
    string Frequency,
    int DueDay,
    int LeadDays,
    decimal Amount,
    bool IsVariableAmount,
    DateOnly StartDate,
    DateOnly? EndDate = null,
    int? MaxOccurrences = null) : IRequest<Result<ExpenseRecurrenceResponse>>
{
    /// <summary>
    /// <see cref="Frequency"/> as the enum, or null when it names nothing. Parsed in one place so
    /// the validator's day-of-week rule and the handler cannot disagree about what was sent.
    /// </summary>
    public RecurrenceFrequency? ParsedFrequency =>
        ExpenseRecurrenceResponse.ParseFrequency(Frequency);
}
