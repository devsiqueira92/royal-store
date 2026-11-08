using System.Text.Json.Serialization;
using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Customers.DTOs;

/// <summary><c>Addresses</c> is filled only by customer details and registration; everywhere else it
/// stays null and the key is left out of the JSON, so those responses are unchanged (045 FR-004).</summary>
public record CustomerResponse(
    Guid Id,
    string Name,
    string? Document,
    string? Phone,
    DateOnly? BirthDate,
    Guid LoyaltyTierId,
    string LoyaltyTierName,
    decimal LoyaltyTierCashbackPercentage,
    bool IsActive,
    decimal? CashbackBalance,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IReadOnlyList<CustomerAddressResponse>? Addresses = null)
{
    public static CustomerResponse From(Customer customer, LoyaltyTier tier, decimal? cashbackBalance = null,
        IReadOnlyList<CustomerAddressResponse>? addresses = null) =>
        new(
            customer.Id,
            customer.Name,
            customer.Document,
            customer.Phone,
            customer.BirthDate,
            customer.LoyaltyTierId,
            tier.Name,
            tier.CashbackPercentage,
            customer.IsActive,
            cashbackBalance,
            addresses);
}
