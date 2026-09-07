using MediatR;
using RoyalStore.Application.OriginOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.OriginOptions.Queries.GetOriginOptionByCode;

public record GetOriginOptionByCodeQuery(string Code) : IRequest<Result<OriginOptionResponse>>;
