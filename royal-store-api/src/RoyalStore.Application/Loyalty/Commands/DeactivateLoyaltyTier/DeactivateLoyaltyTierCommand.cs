using MediatR;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.DeactivateLoyaltyTier;

public record DeactivateLoyaltyTierCommand(Guid Id) : IRequest<Result<LoyaltyTierResponse>>;
