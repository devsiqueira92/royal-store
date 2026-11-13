using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetCostCenters;

public record GetCostCentersQuery(bool IncludeInactive = false)
    : IRequest<Result<IReadOnlyList<CostCenterResponse>>>;
