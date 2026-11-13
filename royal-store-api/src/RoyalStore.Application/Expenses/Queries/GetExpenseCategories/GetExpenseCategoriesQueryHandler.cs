using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetExpenseCategories;

public sealed class GetExpenseCategoriesQueryHandler
    : IRequestHandler<GetExpenseCategoriesQuery, Result<IReadOnlyList<ExpenseCategoryResponse>>>
{
    private readonly IExpenseCategoryRepository _categoryRepository;

    public GetExpenseCategoriesQueryHandler(IExpenseCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IReadOnlyList<ExpenseCategoryResponse>>> Handle(
        GetExpenseCategoriesQuery query, CancellationToken cancellationToken)
    {
        // One flat read, tree assembled in memory (research Decision 5).
        var categories = await _categoryRepository.GetAllAsync(
            query.IncludeInactive ? null : true, cancellationToken);

        return Result<IReadOnlyList<ExpenseCategoryResponse>>.Success(
            ExpenseCategoryResponse.BuildTree(categories));
    }
}
