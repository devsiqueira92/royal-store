using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.UpdateExpenseRecurrence;

/// <summary>
/// Edits a template. Only cycles generated after the edit reflect it — bills already created are
/// untouched (FR-038), because generation copied these values onto them.
/// </summary>
/// <param name="Frequency">A frequency name, as on <see cref="CreateExpenseRecurrence.CreateExpenseRecurrenceCommand"/>.</param>
public record UpdateExpenseRecurrenceCommand(
    Guid Id,
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
    /// <inheritdoc cref="CreateExpenseRecurrence.CreateExpenseRecurrenceCommand.ParsedFrequency"/>
    public RecurrenceFrequency? ParsedFrequency =>
        ExpenseRecurrenceResponse.ParseFrequency(Frequency);
}
