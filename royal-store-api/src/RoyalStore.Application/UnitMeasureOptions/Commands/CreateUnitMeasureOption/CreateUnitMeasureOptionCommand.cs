using MediatR;
using RoyalStore.Application.UnitMeasureOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.UnitMeasureOptions.Commands.CreateUnitMeasureOption;

public record CreateUnitMeasureOptionCommand(string Code, string Name, string Description)
    : IRequest<Result<UnitMeasureOptionResponse>>;
