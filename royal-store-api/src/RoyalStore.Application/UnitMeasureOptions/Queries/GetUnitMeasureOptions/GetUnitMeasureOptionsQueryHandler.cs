using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.UnitMeasureOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.UnitMeasureOptions.Queries.GetUnitMeasureOptions;

public sealed class GetUnitMeasureOptionsQueryHandler
    : IRequestHandler<GetUnitMeasureOptionsQuery, Result<IReadOnlyList<UnitMeasureOptionResponse>>>
{
    private readonly IUnitMeasureOptionRepository _repository;

    public GetUnitMeasureOptionsQueryHandler(IUnitMeasureOptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<UnitMeasureOptionResponse>>> Handle(
        GetUnitMeasureOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var options = await _repository.GetAllAsync(cancellationToken);

        IReadOnlyList<UnitMeasureOptionResponse> response = options
            .Select(UnitMeasureOptionResponse.From)
            .ToList();

        return Result<IReadOnlyList<UnitMeasureOptionResponse>>.Success(response);
    }
}
