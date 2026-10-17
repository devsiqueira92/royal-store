namespace RoyalStore.Application.Sales.DTOs;

/// <summary>How and from where the sale is fulfilled. <c>BranchId</c> is an opaque store-location
/// reference (no branch entity exists yet); required only when <c>Mode</c> is "pickup".
/// <c>Address</c> is optional for both modes.</summary>
public record RegisterSaleFulfillment(string Mode, string? BranchId, RegisterSaleAddress? Address = null);
