namespace RoyalStore.Application.Sales.DTOs;

/// <summary>The money figures exactly as the terminal presented them to the customer.</summary>
public record RegisterSaleTotals(
    decimal Subtotal,
    decimal Discount,
    string? DiscountMode,
    decimal AdditionalFees,
    decimal Total,
    decimal Change);
