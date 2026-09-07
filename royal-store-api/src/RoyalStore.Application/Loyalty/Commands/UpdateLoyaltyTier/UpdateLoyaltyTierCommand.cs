using MediatR;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.UpdateLoyaltyTier;

public record UpdateLoyaltyTierCommand(
    Guid Id,
    decimal CashbackPercentage,
    int CashbackValidityDays) : IRequest<Result<LoyaltyTierResponse>>;
