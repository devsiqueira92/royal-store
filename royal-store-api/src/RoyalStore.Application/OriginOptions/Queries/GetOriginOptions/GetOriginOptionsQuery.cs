using MediatR;
using RoyalStore.Application.OriginOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.OriginOptions.Queries.GetOriginOptions;

public record GetOriginOptionsQuery : IRequest<Result<IReadOnlyList<OriginOptionResponse>>>;
