namespace RoyalStore.Application.Sales.DTOs;

/// <summary>The checkout screen's own totals breakdown. Of all these figures, only
/// <see cref="SaleDiscount"/> is a genuine input to the server's own calculation (a manual,
/// cashier-entered discount with no other source); <see cref="Subtotal"/> is cross-checked against
/// the server-priced item lines. Every other field is informational only — received so the contract
/// matches the wire payload exactly, never trusted as submitted (research.md Decision 8).</summary>
public record RegisterSaleTotals(
    decimal Subtotal,
    decimal SaleDiscount,
    decimal Base,
    decimal LoyaltyRate,
    decimal LoyaltyDiscount,
    decimal PaymentRate,
    decimal PaymentDiscount,
    decimal CashbackApplied,
    decimal DiscountTotal,
    decimal Tax,
    decimal Total);
