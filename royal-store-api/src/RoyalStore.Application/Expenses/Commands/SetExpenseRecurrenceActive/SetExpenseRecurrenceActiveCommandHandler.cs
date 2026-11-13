using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.SetExpenseRecurrenceActive;

public sealed class SetExpenseRecurrenceActiveCommandHandler
    : IRequestHandler<SetExpenseRecurrenceActiveCommand, Result<ExpenseRecurrenceResponse>>
{
    private readonly IExpenseRecurrenceRepository _recurrenceRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetExpenseRecurrenceActiveCommandHandler(
        IExpenseRecurrenceRepository recurrenceRepository,
        IExpenseCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _recurrenceRepository = recurrenceRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExpenseRecurrenceResponse>> Handle(
        SetExpenseRecurrenceActiveCommand command, CancellationToken cancellationToken)
    {
        var recurrence = await _recurrenceRepository.GetByIdAsync(command.Id, cancellationToken);
        if (recurrence is null)
            return Error.NotFound("expenserecurrence.not_found", "Recurring template was not found.");

        var transition = command.IsActive ? recurrence.Activate() : recurrence.Deactivate();
        if (transition.IsFailure)
            return transition.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var category = await _categoryRepository.GetByIdAsync(recurrence.ExpenseCategoryId, cancellationToken);

        return ExpenseRecurrenceResponse.From(
            recurrence,
            category is null
                ? new ExpenseCategorySummary(recurrence.ExpenseCategoryId, string.Empty, null)
                : ExpenseCategorySummary.From(category));
    }
}
