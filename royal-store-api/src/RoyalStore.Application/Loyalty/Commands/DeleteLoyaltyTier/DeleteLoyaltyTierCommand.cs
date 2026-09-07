using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.DeleteLoyaltyTier;

public record DeleteLoyaltyTierCommand(Guid Id) : IRequest<Result>;
