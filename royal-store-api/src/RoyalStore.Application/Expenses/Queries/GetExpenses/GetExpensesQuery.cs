using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Queries.GetExpenses;

/// <summary>
/// The competence-month view: four headline figures plus the filtered list (FR-041–FR-043).
/// </summary>
public record GetExpensesQuery(
    DateOnly? CompetenceMonth = null,
    ExpenseStatus? Status = null,
    bool? Overdue = null,
    Guid? CategoryId = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<ExpenseMonthResponse>>;
