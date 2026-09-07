using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Customers.DTOs;

public record CustomerResponse(
    Guid Id,
    string Name,
    string Document,
    string? Phone,
    Guid LoyaltyTierId,
    string LoyaltyTierName,
    bool IsActive,
    decimal CashbackBalance)
{
    public static CustomerResponse From(Customer customer, LoyaltyTier tier, decimal cashbackBalance) =>
        new(
            customer.Id,
            customer.Name,
            customer.Document,
            customer.Phone,
            customer.LoyaltyTierId,
            tier.Name,
            customer.IsActive,
            cashbackBalance);
}
