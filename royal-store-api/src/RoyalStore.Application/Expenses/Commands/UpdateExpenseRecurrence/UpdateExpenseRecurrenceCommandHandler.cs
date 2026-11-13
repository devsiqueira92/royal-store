using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.Common;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.UpdateExpenseRecurrence;

public sealed class UpdateExpenseRecurrenceCommandHandler
    : IRequestHandler<UpdateExpenseRecurrenceCommand, Result<ExpenseRecurrenceResponse>>
{
    private readonly IExpenseRecurrenceRepository _recurrenceRepository;
    private readonly ExpenseClassificationGuard _classificationGuard;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateExpenseRecurrenceCommandHandler(
        IExpenseRecurrenceRepository recurrenceRepository,
        ExpenseClassificationGuard classificationGuard,
        IUnitOfWork unitOfWork)
    {
        _recurrenceRepository = recurrenceRepository;
        _classificationGuard = classificationGuard;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExpenseRecurrenceResponse>> Handle(
        UpdateExpenseRecurrenceCommand command, CancellationToken cancellationToken)
    {
        // See the create handler: the domain is never reached with an unparseable frequency.
        if (command.ParsedFrequency is not { } frequency)
        {
            return Error.Validation(
                "expenserecurrence.frequency_invalid",
                $"Frequency must be one of: {ExpenseRecurrenceResponse.FrequencyNames}.");
        }

        var recurrence = await _recurrenceRepository.GetByIdAsync(command.Id, cancellationToken);
        if (recurrence is null)
            return Error.NotFound("expenserecurrence.not_found", "Recurring template was not found.");

        var classification = await _classificationGuard.ValidateAsync(
            command.ExpenseCategoryId, command.CostCenterId, command.FinancialAccountId,
            requireActive: true, cancellationToken);

        if (classification.IsFailure)
            return classification.Error;

        var update = recurrence.Update(
            command.Description,
            command.PayeeName,
            command.SupplierId,
            command.ExpenseCategoryId,
            command.CostCenterId,
            command.FinancialAccountId,
            frequency,
            command.DueDay,
            command.LeadDays,
            command.Amount,
            command.IsVariableAmount,
            command.StartDate,
            command.EndDate,
            command.MaxOccurrences);

        if (update.IsFailure)
            return update.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseRecurrenceResponse.From(
            recurrence, ExpenseCategorySummary.From(classification.Value));
    }
}
