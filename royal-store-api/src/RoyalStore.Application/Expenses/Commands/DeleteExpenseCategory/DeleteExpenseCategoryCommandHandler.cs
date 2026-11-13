using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.DeleteExpenseCategory;

public sealed class DeleteExpenseCategoryCommandHandler
    : IRequestHandler<DeleteExpenseCategoryCommand, Result>
{
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExpenseCategoryCommandHandler(
        IExpenseCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteExpenseCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("expensecategory.not_found",
                "Expense category was not found."));

        // Checked here so the caller gets an explanation and the deactivate alternative, rather
        // than a raw FK violation. The Restrict FKs are the backstop, not the message.
        if (await _categoryRepository.IsReferencedAsync(command.Id, cancellationToken))
            return Result.Failure(Error.Conflict("expensecategory.in_use",
                "This category is in use by an expense, a template or a child category. Deactivate it instead."));

        await _categoryRepository.RemoveAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
