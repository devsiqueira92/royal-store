using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.DeleteExpenseCategory;

/// <summary>Deletes an unused category. One still referenced is refused (FR-027).</summary>
public record DeleteExpenseCategoryCommand(Guid Id) : IRequest<Result>;
