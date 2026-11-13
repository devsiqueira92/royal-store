using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.SetExpenseCategoryActive;

public sealed class SetExpenseCategoryActiveCommandHandler
    : IRequestHandler<SetExpenseCategoryActiveCommand, Result<ExpenseCategoryResponse>>
{
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetExpenseCategoryActiveCommandHandler(
        IExpenseCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExpenseCategoryResponse>> Handle(
        SetExpenseCategoryActiveCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Error.NotFound("expensecategory.not_found", "Expense category was not found.");

        var transition = command.IsActive ? category.Activate() : category.Deactivate();
        if (transition.IsFailure)
            return transition.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseCategoryResponse.From(category, []);
    }
}
