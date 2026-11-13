using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.CreateExpenseCategory;

public sealed class CreateExpenseCategoryCommandHandler
    : IRequestHandler<CreateExpenseCategoryCommand, Result<ExpenseCategoryResponse>>
{
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateExpenseCategoryCommandHandler(
        IExpenseCategoryRepository categoryRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ExpenseCategoryResponse>> Handle(
        CreateExpenseCategoryCommand command, CancellationToken cancellationToken)
    {
        if (command.ParentId is not null)
        {
            var parent = await _categoryRepository.GetByIdAsync(command.ParentId.Value, cancellationToken);
            if (parent is null)
                return Error.NotFound("expensecategory.parent_not_found", "Parent category was not found.");
        }

        var name = command.Name.Trim();

        if (await _categoryRepository.ExistsByNameAsync(name, command.ParentId, null, cancellationToken))
            return Error.Conflict("expensecategory.duplicate_name",
                "A category with this name already exists at this level.");

        var categoryResult = ExpenseCategory.Create(
            _tenantContext.TenantId, name, command.ParentId, command.Color, command.DisplayOrder);

        if (categoryResult.IsFailure)
            return categoryResult.Error;

        await _categoryRepository.AddAsync(categoryResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ExpenseCategoryResponse.From(categoryResult.Value, []);
    }
}
