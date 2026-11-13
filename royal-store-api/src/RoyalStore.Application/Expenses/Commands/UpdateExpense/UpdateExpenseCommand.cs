using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.UpdateExpense;

/// <summary>
/// Edits an expense (FR-005). The amount is routed through Expense.ChangeAmount, which refuses to
/// drop it below what is already settled (FR-006) and clears the estimate flag (FR-004) — editing a
/// generated bill to its real invoiced value is the main reason this command exists.
/// </summary>
public record UpdateExpenseCommand(
    Guid Id,
    string Description,
    string? PayeeName,
    Guid? SupplierId,
    Guid ExpenseCategoryId,
    Guid? CostCenterId,
    Guid? FinancialAccountId,
    decimal Amount,
    DateOnly CompetenceMonth,
    DateOnly DueDate,
    string? Notes = null) : IRequest<Result<ExpenseResponse>>;
