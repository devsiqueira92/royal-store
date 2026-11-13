using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.ReverseExpensePayment;

/// <summary>
/// Reverses a settlement (FR-018). The payment is flagged rather than deleted, so the history of
/// what was recorded and undone survives.
/// </summary>
public record ReverseExpensePaymentCommand(
    Guid ExpenseId,
    Guid PaymentId,
    string? Reason = null) : IRequest<Result<ExpenseResponse>>;
