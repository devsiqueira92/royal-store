using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.RegisterExpensePayment;

/// <summary>
/// Settles part or all of an expense (FR-012). Returns the whole expense so the caller sees the
/// recalculated status and balance without a second round trip.
/// </summary>
public record RegisterExpensePaymentCommand(
    Guid ExpenseId,
    decimal Amount,
    DateOnly PaidAt,
    Guid FinancialAccountId,
    string? MethodNote = null,
    string? Notes = null) : IRequest<Result<ExpenseResponse>>;
