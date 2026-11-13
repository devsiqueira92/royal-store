using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetExpenseCategories;

/// <summary>The category hierarchy (FR-022). Inactive categories are hidden unless asked for.</summary>
public record GetExpenseCategoriesQuery(bool IncludeInactive = false)
    : IRequest<Result<IReadOnlyList<ExpenseCategoryResponse>>>;
