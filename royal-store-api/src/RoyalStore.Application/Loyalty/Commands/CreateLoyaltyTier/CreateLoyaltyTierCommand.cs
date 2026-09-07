using MediatR;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.CreateLoyaltyTier;

public record CreateLoyaltyTierCommand(
    string Name,
    decimal CashbackPercentage,
    int CashbackValidityDays) : IRequest<Result<LoyaltyTierResponse>>;
