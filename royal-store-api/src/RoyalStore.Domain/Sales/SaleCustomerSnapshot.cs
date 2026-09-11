namespace RoyalStore.Domain.Sales;

/// <summary>
/// The customer-related facts frozen onto a sale at the moment it was completed. Deliberately
/// excludes document and phone number — this type makes that omission structurally impossible to
/// violate by accident, not merely a convention.
/// </summary>
public sealed record SaleCustomerSnapshot(string Name, string LoyaltyTierName);
