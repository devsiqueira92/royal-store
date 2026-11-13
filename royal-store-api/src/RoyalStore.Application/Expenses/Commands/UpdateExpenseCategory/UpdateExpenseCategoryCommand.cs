using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.UpdateExpenseCategory;

/// <summary>
/// Renames or re-parents a category. A parent change that would close a loop is refused
/// (FR-024).
/// </summary>
public record UpdateExpenseCategoryCommand(
    Guid Id,
    string Name,
    Guid? ParentId = null,
    string? Color = null,
    int DisplayOrder = 0) : IRequest<Result<ExpenseCategoryResponse>>;
