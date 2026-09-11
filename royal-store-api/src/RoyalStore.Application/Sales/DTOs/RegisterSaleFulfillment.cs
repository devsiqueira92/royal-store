namespace RoyalStore.Application.Sales.DTOs;

/// <summary>How and from where the sale is fulfilled. <c>BranchId</c> is an opaque store-location
/// reference (no branch entity exists yet); required only when <c>Mode</c> is "pickup".</summary>
public record RegisterSaleFulfillment(string Mode, string? BranchId);
