using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

/// <summary>
/// A product's price on one price list, with enough of the list to label it (061 FR-014) and, when
/// the list has a percentage rule, what that rule suggests today (062 FR-019, FR-020).
/// </summary>
/// <param name="SalePrice">The stored price — always what was saved, never a calculation.</param>
/// <param name="DiscountPercentage">The list's rule as it is now: negative = discount, positive =
/// surcharge; null = no rule.</param>
/// <param name="BasePriceListId">The list the rule is calculated from; null = no rule.</param>
/// <param name="SuggestedPrice">What the rule yields now for this product; null when
/// <paramref name="RuleStatus"/> is <c>none</c>.</param>
/// <param name="RuleStatus"><c>none</c> (no rule, or the product has no price on the base list),
/// <c>matches</c> (stored price equals the suggestion) or <c>differs</c>. Computed on read.</param>
public record ProductPriceResponse(
    Guid PriceListId,
    string PriceListName,
    bool IsDefault,
    bool IsActive,
    decimal SalePrice,
    decimal? MinPrice,
    decimal? DiscountPercentage,
    Guid? BasePriceListId,
    decimal? SuggestedPrice,
    string RuleStatus)
{
    public static ProductPriceResponse From(ProductPrice price, Product owner)
    {
        var rule = owner.EvaluatePriceRule(price);

        return new(price.PriceListId, price.PriceList.Name, price.PriceList.IsDefault, price.PriceList.IsActive,
            price.SalePrice, price.MinPrice,
            price.PriceList.DiscountPercentage,
            price.PriceList.BasePriceListId,
            rule.SuggestedPrice,
            rule.Status.ToString().ToLowerInvariant());
    }
}
