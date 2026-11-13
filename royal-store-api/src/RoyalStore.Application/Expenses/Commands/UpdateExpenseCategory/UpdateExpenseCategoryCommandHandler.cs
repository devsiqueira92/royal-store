using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.UpdateExpenseCategory;

public sealed class UpdateExpenseCategoryCommandHandler
    : IRequestHandler<UpdateExpenseCategoryCommand, Result<ExpenseCategoryResponse>>
{
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateExpenseCategoryCommandHandler(
        IExpenseCategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExpenseCategoryResponse>> Handle(
        UpdateExpenseCategoryCommand command, CancellationToken cancellationToken)
    {
        // The whole set is loaded because ChangeParent walks ancestors in memory to detect a cycle.
        var all = await _categoryRepository.GetAllAsync(ct: cancellationToken);

        var category = all.FirstOrDefault(c => c.Id == command.Id);
        if (category is null)
            return Error.NotFound("expensecategory.not_found", "Expense category was not found.");

        var name = command.Name.Trim();

        if (await _categoryRepository.ExistsByNameAsync(name, command.ParentId, command.Id, cancellationToken))
            return Error.Conflict("expensecategory.duplicate_name",
                "A category with this name already exists at this level.");

        if (command.ParentId != category.ParentId)
        {
            var reparent = category.ChangeParent(command.ParentId, all);
            if (reparent.IsFailure)
                return reparent.Error;
        }

        var update = category.Update(name, command.Color, command.DisplayOrder);
        if (update.IsFailure)
            return update.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseCategoryResponse.From(category, []);
    }
}
