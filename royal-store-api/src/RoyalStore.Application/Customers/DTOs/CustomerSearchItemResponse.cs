using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Customers.DTOs;

/// <summary>A customer as the checkout lists it: who they are and what their tier earns back. No PII
/// beyond the name — the search matches on document and phone but never returns them.</summary>
public record CustomerSearchItemResponse(
    Guid Id,
    string Name,
    Guid LoyaltyTierId,
    string LoyaltyTierName,
    decimal LoyaltyTierCashbackPercentage)
{
    public static CustomerSearchItemResponse From(Customer customer, LoyaltyTier tier) =>
        new(customer.Id, customer.Name, tier.Id, tier.Name, tier.CashbackPercentage);
}
