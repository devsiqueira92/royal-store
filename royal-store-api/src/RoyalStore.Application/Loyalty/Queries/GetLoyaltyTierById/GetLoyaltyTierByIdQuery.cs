using MediatR;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Queries.GetLoyaltyTierById;

public record GetLoyaltyTierByIdQuery(Guid Id) : IRequest<Result<LoyaltyTierResponse>>;
