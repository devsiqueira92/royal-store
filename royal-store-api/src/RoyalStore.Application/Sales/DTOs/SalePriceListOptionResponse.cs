namespace RoyalStore.Application.Sales.DTOs;

/// <summary>A price list the sale can be charged on, with the cart priced on it.</summary>
/// <param name="Subtotal">Sum of <see cref="Lines"/>' totals — the checkout's subtotal on this list.</param>
/// <param name="Lines">Per-line prices, so the terminal can recompute tax and discounts.</param>
public record SalePriceListOptionResponse(
    Guid PriceListId,
    string Name,
    bool IsDefault,
    decimal Subtotal,
    IReadOnlyList<SalePriceListLineResponse> Lines);

public record SalePriceListLineResponse(Guid VariantId, decimal UnitPrice, decimal LineTotal);
