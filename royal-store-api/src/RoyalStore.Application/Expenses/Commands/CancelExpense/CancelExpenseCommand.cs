using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.CancelExpense;

/// <summary>Cancels an expense recorded by mistake (FR-007). Refused once money has been settled against it.</summary>
public record CancelExpenseCommand(Guid Id) : IRequest<Result<ExpenseResponse>>;
