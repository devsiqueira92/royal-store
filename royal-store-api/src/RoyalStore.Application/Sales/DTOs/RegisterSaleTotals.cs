namespace RoyalStore.Application.Sales.DTOs;

/// <summary>The money figures exactly as the terminal presented them to the customer.</summary>
public record RegisterSaleTotals(
    decimal Subtotal,
    decimal Discount,
    string? DiscountMode,
    decimal AdditionalFees,
    decimal Total,
    decimal Change,
    /// <summary>How much of the customer's cashback balance to apply (FR-023). A request, not an
    /// instruction: the server clamps this to what is actually usable and to the amount due
    /// (FR-027) — see <c>RegisterSaleResponse.CashbackApplied</c> for what was actually applied.</summary>
    decimal CashbackRequested = 0m);
