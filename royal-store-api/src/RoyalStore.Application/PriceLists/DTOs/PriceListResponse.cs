using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.PriceLists.DTOs;

/// <param name="ProductCount">How many products are priced on the list.</param>
/// <param name="DiscountPercentage">The list's percentage rule: negative = discount, positive =
/// surcharge. Null (together with the base list) = no rule, priced by hand.</param>
/// <param name="BasePriceListId">The list the rule is calculated from.</param>
/// <param name="BasePriceListName">Name of that list.</param>
/// <param name="DependentListCount">How many lists are based on this one. Above zero, this list
/// cannot receive a rule, be deactivated or be deleted.</param>
public record PriceListResponse(
    Guid Id,
    string Name,
    bool IsDefault,
    bool IsActive,
    int ProductCount,
    decimal? DiscountPercentage,
    Guid? BasePriceListId,
    string? BasePriceListName,
    int DependentListCount)
{
    /// <param name="storeLists">Every price list of the store, active or not — where the base
    /// list's name and the dependents are looked up.</param>
    public static PriceListResponse From(
        PriceList priceList, int productCount, IReadOnlyList<PriceList> storeLists) =>
        new(priceList.Id, priceList.Name, priceList.IsDefault, priceList.IsActive, productCount,
            priceList.DiscountPercentage,
            priceList.BasePriceListId,
            storeLists.FirstOrDefault(l => l.Id == priceList.BasePriceListId)?.Name,
            storeLists.Count(l => l.BasePriceListId == priceList.Id));
}
