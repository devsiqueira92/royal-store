using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Customers.DTOs;

public record CustomerResponse(
    Guid Id,
    string Name,
    string? Phone,
    Guid LoyaltyTierId,
    string LoyaltyTierName,
    decimal LoyaltyTierCashbackPercentage,
    bool IsActive,
    decimal? CashbackBalance)
{
    public static CustomerResponse From(Customer customer, LoyaltyTier tier, decimal? cashbackBalance = null) =>
        new(
            customer.Id,
            customer.Name,
            customer.Phone,
            customer.LoyaltyTierId,
            tier.Name,
            tier.CashbackPercentage,
            customer.IsActive,
            cashbackBalance);
}
