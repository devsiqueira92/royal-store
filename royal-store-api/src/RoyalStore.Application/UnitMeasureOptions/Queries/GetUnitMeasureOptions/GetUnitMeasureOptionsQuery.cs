using MediatR;
using RoyalStore.Application.UnitMeasureOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.UnitMeasureOptions.Queries.GetUnitMeasureOptions;

public record GetUnitMeasureOptionsQuery : IRequest<Result<IReadOnlyList<UnitMeasureOptionResponse>>>;
