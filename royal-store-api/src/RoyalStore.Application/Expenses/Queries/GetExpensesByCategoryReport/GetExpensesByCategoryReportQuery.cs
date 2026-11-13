using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetExpensesByCategoryReport;

/// <summary>
/// Expenses by category over the <paramref name="Months"/> competence months ending at
/// <paramref name="CompetenceMonth"/> — the management P&amp;L (FR-044).
/// </summary>
public record GetExpensesByCategoryReportQuery(
    DateOnly? CompetenceMonth = null,
    int Months = 6) : IRequest<Result<ExpensesByCategoryReportResponse>>;
