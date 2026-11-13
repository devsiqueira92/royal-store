using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.DeleteExpenseRecurrence;

public sealed class DeleteExpenseRecurrenceCommandHandler
    : IRequestHandler<DeleteExpenseRecurrenceCommand, Result>
{
    private readonly IExpenseRecurrenceRepository _recurrenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExpenseRecurrenceCommandHandler(
        IExpenseRecurrenceRepository recurrenceRepository,
        IUnitOfWork unitOfWork)
    {
        _recurrenceRepository = recurrenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteExpenseRecurrenceCommand command, CancellationToken cancellationToken)
    {
        var recurrence = await _recurrenceRepository.GetByIdAsync(command.Id, cancellationToken);
        if (recurrence is null)
            return Result.Failure(Error.NotFound("expenserecurrence.not_found",
                "Recurring template was not found."));

        if (recurrence.OccurrencesGenerated > 0)
            return Result.Failure(Error.Conflict("expenserecurrence.has_generated_expenses",
                "This template has already generated expenses. Deactivate it instead of deleting it."));

        await _recurrenceRepository.RemoveAsync(recurrence, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
