using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.PriceLists.DTOs;

/// <param name="ProductCount">How many products are priced on the list.</param>
public record PriceListResponse(
    Guid Id,
    string Name,
    bool IsDefault,
    bool IsActive,
    int ProductCount)
{
    public static PriceListResponse From(PriceList priceList, int productCount) =>
        new(priceList.Id, priceList.Name, priceList.IsDefault, priceList.IsActive, productCount);
}
