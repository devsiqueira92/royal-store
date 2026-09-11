namespace RoyalStore.Application.Sales.DTOs;

public record RegisterSaleResponse(
    Guid SaleId,
    SyncStatus Status,
    decimal CashbackApplied = 0m,
    decimal CashbackEarned = 0m,
    /// <summary>How much the resolved payment method's discount reduced this sale's total (037).</summary>
    decimal PaymentDiscountApplied = 0m,
    /// <summary>How much the customer's loyalty tier reduced this sale's total.</summary>
    decimal LoyaltyDiscountApplied = 0m,
    /// <summary>The server-computed tax amount included in the total.</summary>
    decimal TaxAmount = 0m,
    /// <summary>The server's authoritative, persisted total — the figure downstream fiscal document
    /// generation uses, regardless of what the request's own totals said.</summary>
    decimal TotalAmount = 0m);
