using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

/// <summary>A product's price on one price list, with enough of the list to label it (061 FR-014).</summary>
public record ProductPriceResponse(
    Guid PriceListId,
    string PriceListName,
    bool IsDefault,
    bool IsActive,
    decimal SalePrice,
    decimal? MinPrice)
{
    public static ProductPriceResponse From(ProductPrice price) =>
        new(price.PriceListId, price.PriceList.Name, price.PriceList.IsDefault, price.PriceList.IsActive,
            price.SalePrice, price.MinPrice);
}
