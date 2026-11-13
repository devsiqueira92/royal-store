using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.CreateExpense;

/// <summary>
/// Records an operating expense (FR-001). This module accepts operating cost only — merchandise for
/// resale and fixed assets belong to inventory and capex, never here (FR-011).
/// </summary>
public record CreateExpenseCommand(
    string Description,
    string? PayeeName,
    Guid? SupplierId,
    Guid ExpenseCategoryId,
    Guid? CostCenterId,
    Guid? FinancialAccountId,
    decimal Amount,
    DateOnly CompetenceMonth,
    DateOnly DueDate,
    bool IsEstimated = false,
    string? Notes = null) : IRequest<Result<ExpenseResponse>>;
