using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetExpenseCashFlowProjection;

/// <summary>
/// What the store owes over the weeks ahead, split into overdue, committed (bills that exist) and
/// projected (cycles templates will still generate) — FR-045.
/// </summary>
public record GetExpenseCashFlowProjectionQuery(int Weeks = 8)
    : IRequest<Result<CashFlowProjectionResponse>>;
