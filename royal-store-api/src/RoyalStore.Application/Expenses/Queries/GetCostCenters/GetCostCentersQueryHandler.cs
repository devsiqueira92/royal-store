using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetCostCenters;

public sealed class GetCostCentersQueryHandler
    : IRequestHandler<GetCostCentersQuery, Result<IReadOnlyList<CostCenterResponse>>>
{
    private readonly ICostCenterRepository _costCenterRepository;

    public GetCostCentersQueryHandler(ICostCenterRepository costCenterRepository)
    {
        _costCenterRepository = costCenterRepository;
    }

    public async Task<Result<IReadOnlyList<CostCenterResponse>>> Handle(
        GetCostCentersQuery query, CancellationToken cancellationToken)
    {
        var costCenters = await _costCenterRepository.GetAllAsync(
            query.IncludeInactive ? null : true, cancellationToken);

        return Result<IReadOnlyList<CostCenterResponse>>.Success(
            [.. costCenters.Select(CostCenterResponse.From)]);
    }
}
