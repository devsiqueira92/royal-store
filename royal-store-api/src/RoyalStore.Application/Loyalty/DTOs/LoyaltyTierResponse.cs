using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Loyalty.DTOs;

public record LoyaltyTierResponse(Guid Id, string Name, decimal CashbackPercentage, int CashbackValidityDays, bool IsActive)
{
    public static LoyaltyTierResponse From(LoyaltyTier tier) =>
        new(tier.Id, tier.Name, tier.CashbackPercentage, tier.CashbackValidityDays, tier.Active);
}
