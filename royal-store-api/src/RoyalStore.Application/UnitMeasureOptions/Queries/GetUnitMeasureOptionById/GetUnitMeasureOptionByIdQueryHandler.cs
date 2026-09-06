using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.UnitMeasureOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.UnitMeasureOptions.Queries.GetUnitMeasureOptionById;

public sealed class GetUnitMeasureOptionByIdQueryHandler
    : IRequestHandler<GetUnitMeasureOptionByIdQuery, Result<UnitMeasureOptionResponse>>
{
    private readonly IUnitMeasureOptionRepository _repository;

    public GetUnitMeasureOptionByIdQueryHandler(IUnitMeasureOptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UnitMeasureOptionResponse>> Handle(
        GetUnitMeasureOptionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var option = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (option is null)
            return Error.NotFound("unit_measure_option.not_found", "Unit measure option not found.");

        return UnitMeasureOptionResponse.From(option);
    }
}
