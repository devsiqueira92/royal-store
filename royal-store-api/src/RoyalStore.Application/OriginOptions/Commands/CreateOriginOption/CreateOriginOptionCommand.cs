using MediatR;
using RoyalStore.Application.OriginOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.OriginOptions.Commands.CreateOriginOption;

public record CreateOriginOptionCommand(string Code, string Name, string Description)
    : IRequest<Result<OriginOptionResponse>>;
