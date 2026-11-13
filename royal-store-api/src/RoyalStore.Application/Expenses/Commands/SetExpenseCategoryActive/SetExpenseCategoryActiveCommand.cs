using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.SetExpenseCategoryActive;

/// <summary>
/// Activates or deactivates a category (FR-028). Deactivating hides it from new entries while every
/// expense already attributed to it keeps its classification.
/// </summary>
public record SetExpenseCategoryActiveCommand(Guid Id, bool IsActive)
    : IRequest<Result<ExpenseCategoryResponse>>;
