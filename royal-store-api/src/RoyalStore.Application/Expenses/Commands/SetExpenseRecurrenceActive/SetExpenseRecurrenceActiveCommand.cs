using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.SetExpenseRecurrenceActive;

/// <summary>
/// Activates or deactivates a template (FR-037). Deactivating stops generation while leaving every
/// bill it already produced untouched (FR-035).
/// </summary>
public record SetExpenseRecurrenceActiveCommand(Guid Id, bool IsActive)
    : IRequest<Result<ExpenseRecurrenceResponse>>;
