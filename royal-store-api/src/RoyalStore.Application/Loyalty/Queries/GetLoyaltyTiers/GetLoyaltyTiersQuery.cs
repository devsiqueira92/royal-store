using MediatR;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Queries.GetLoyaltyTiers;

public record GetLoyaltyTiersQuery : IRequest<Result<IReadOnlyList<LoyaltyTierResponse>>>;
