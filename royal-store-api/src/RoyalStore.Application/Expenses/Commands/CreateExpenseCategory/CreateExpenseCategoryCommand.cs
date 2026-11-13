using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.CreateExpenseCategory;

/// <summary>Adds a category, optionally beneath a parent (FR-022).</summary>
public record CreateExpenseCategoryCommand(
    string Name,
    Guid? ParentId = null,
    string? Color = null,
    int DisplayOrder = 0) : IRequest<Result<ExpenseCategoryResponse>>;
