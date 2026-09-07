using MediatR;
using RoyalStore.Application.UnitMeasureOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.UnitMeasureOptions.Commands.UpdateUnitMeasureOption;

public record UpdateUnitMeasureOptionCommand(Guid Id, string Code, string Name, string Description)
    : IRequest<Result<UnitMeasureOptionResponse>>;
