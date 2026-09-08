using MediatR;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.ActivateLoyaltyTier;

public record ActivateLoyaltyTierCommand(Guid Id) : IRequest<Result<LoyaltyTierResponse>>;
