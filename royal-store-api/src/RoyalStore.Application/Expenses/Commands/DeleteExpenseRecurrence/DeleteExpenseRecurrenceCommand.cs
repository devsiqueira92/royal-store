using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.DeleteExpenseRecurrence;

/// <summary>
/// Deletes a template that has never generated a bill (FR-037). One that has is deactivated
/// instead, so the bills it produced keep a template to point at.
/// </summary>
public record DeleteExpenseRecurrenceCommand(Guid Id) : IRequest<Result>;
