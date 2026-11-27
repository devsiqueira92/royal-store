namespace RoyalStore.Application.Sales.DTOs;

/// <summary>A price list the sale can be charged on, with the cart priced on it.</summary>
/// <param name="DiscountPercentage">The list's percentage rule over its base list: negative is a
/// discount, positive a surcharge, null when the list is priced by hand (always null on the
/// default list, which has no base).</param>
/// <param name="Subtotal">Sum of <see cref="Lines"/>' totals — the checkout's subtotal on this list.</param>
/// <param name="Lines">Per-line prices, so the terminal can recompute tax and discounts.</param>
public record SalePriceListOptionResponse(
    Guid PriceListId,
    string Name,
    bool IsDefault,
    decimal? DiscountPercentage,
    decimal Subtotal,
    IReadOnlyList<SalePriceListLineResponse> Lines);

public record SalePriceListLineResponse(Guid VariantId, decimal UnitPrice, decimal LineTotal);
