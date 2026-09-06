using MediatR;
using RoyalStore.Application.UnitMeasureOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.UnitMeasureOptions.Queries.GetUnitMeasureOptionById;

public record GetUnitMeasureOptionByIdQuery(Guid Id) : IRequest<Result<UnitMeasureOptionResponse>>;
