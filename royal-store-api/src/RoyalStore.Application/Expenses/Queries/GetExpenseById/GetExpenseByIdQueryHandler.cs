using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetExpenseById;

public sealed class GetExpenseByIdQueryHandler
    : IRequestHandler<GetExpenseByIdQuery, Result<ExpenseResponse>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;

    public GetExpenseByIdQueryHandler(
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository categoryRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ExpenseResponse>> Handle(
        GetExpenseByIdQuery query, CancellationToken cancellationToken)
    {
        // The aggregate is loaded with Include and shaped in memory rather than projected, so the
        // query is provably translatable (Constitution VI).
        var expense = await _expenseRepository.GetByIdWithPaymentsAsync(query.Id, cancellationToken);

        // Another tenant's expense is filtered out by the global query filter, so it reads as not
        // found here — never as forbidden, which would confirm it exists (FR-009).
        if (expense is null)
            return Error.NotFound("expense.not_found", "Expense was not found.");

        var category = await _categoryRepository.GetByIdAsync(expense.ExpenseCategoryId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return ExpenseResponse.From(
            expense,
            category is null
                ? new ExpenseCategorySummary(expense.ExpenseCategoryId, string.Empty, null)
                : ExpenseCategorySummary.From(category),
            today);
    }
}
