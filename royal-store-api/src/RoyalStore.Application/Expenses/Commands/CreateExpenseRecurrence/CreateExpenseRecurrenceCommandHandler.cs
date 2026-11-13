using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Expenses.Common;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.CreateExpenseRecurrence;

public sealed class CreateExpenseRecurrenceCommandHandler
    : IRequestHandler<CreateExpenseRecurrenceCommand, Result<ExpenseRecurrenceResponse>>
{
    private readonly IExpenseRecurrenceRepository _recurrenceRepository;
    private readonly ExpenseClassificationGuard _classificationGuard;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateExpenseRecurrenceCommandHandler(
        IExpenseRecurrenceRepository recurrenceRepository,
        ExpenseClassificationGuard classificationGuard,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _recurrenceRepository = recurrenceRepository;
        _classificationGuard = classificationGuard;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExpenseRecurrenceResponse>> Handle(
        CreateExpenseRecurrenceCommand command, CancellationToken cancellationToken)
    {
        // Guarded here as well as in the validator: the handler owns the contract with the domain,
        // and must not reach it with a frequency that was never a frequency.
        if (command.ParsedFrequency is not { } frequency)
        {
            return Error.Validation(
                "expenserecurrence.frequency_invalid",
                $"Frequency must be one of: {ExpenseRecurrenceResponse.FrequencyNames}.");
        }

        var classification = await _classificationGuard.ValidateAsync(
            command.ExpenseCategoryId, command.CostCenterId, command.FinancialAccountId,
            requireActive: true, cancellationToken);

        if (classification.IsFailure)
            return classification.Error;

        var recurrenceResult = ExpenseRecurrence.Create(
            _tenantContext.TenantId,
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

        if (recurrenceResult.IsFailure)
            return recurrenceResult.Error;

        await _recurrenceRepository.AddAsync(recurrenceResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseRecurrenceResponse.From(
            recurrenceResult.Value, ExpenseCategorySummary.From(classification.Value));
    }
}
