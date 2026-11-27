using FluentValidation;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products;

/// <summary>
/// Turns the prices a request carries into the price set handed to the <see cref="Product"/>
/// aggregate, and holds the request-shape rules shared by the validators. Every business rule about
/// prices (default list required, inactive lists frozen, ...) lives in the aggregate, not here.
/// </summary>
public static class ProductPriceInputs
{
    private static readonly Error DefaultListMissing = Error.Conflict(
        "pricelist.default_missing",
        "This store has no default price list. Create or choose one before saving products.");

    /// <summary>
    /// The submitted prices, refused when absent or when any list was sent with an empty price —
    /// the error names that list, so the user knows which one to fill in.
    /// </summary>
    public static Result<IReadOnlyList<ProductPriceInput>> Resolve(
        IReadOnlyList<ProductPriceRequest>? prices, IReadOnlyList<PriceList> tenantLists)
    {
        var defaultList = tenantLists.FirstOrDefault(l => l.IsDefault);
        if (defaultList is null)
            return DefaultListMissing;

        if (prices is null || prices.Count == 0)
            return Error.Validation("product.prices_required",
                $"Prices are required: at least the price on the default list '{defaultList.Name}'.");

        var inputs = new List<ProductPriceInput>(prices.Count);

        foreach (var price in prices)
        {
            if (price.SalePrice is not { } salePrice)
            {
                var listName = tenantLists.FirstOrDefault(l => l.Id == price.PriceListId)?.Name
                    ?? price.PriceListId.ToString();

                return Error.Validation("product.price_empty",
                    $"Price list '{listName}' was sent without a sale price. Enter a price or remove the list.");
            }

            inputs.Add(new ProductPriceInput(price.PriceListId, salePrice, price.MinPrice));
        }

        return Result<IReadOnlyList<ProductPriceInput>>.Success(inputs);
    }

    /// <summary>
    /// A single price on the store's default list, for a request that states one price only (a
    /// service). On an edit, every other stored price is resubmitted as it is, so the aggregate's
    /// replace semantics leave it untouched.
    /// </summary>
    public static Result<IReadOnlyList<ProductPriceInput>> ForDefaultList(
        decimal salePrice, Product? existing, IReadOnlyList<PriceList> tenantLists)
    {
        var defaultList = tenantLists.FirstOrDefault(l => l.IsDefault);
        if (defaultList is null)
            return DefaultListMissing;

        var inputs = (existing?.Prices ?? [])
            .Where(p => p.PriceListId != defaultList.Id)
            .Select(p => new ProductPriceInput(p.PriceListId, p.SalePrice, p.MinPrice))
            .ToList();

        // Keep the stored discount floor while it still fits under the new price.
        var storedMin = existing?.Prices.FirstOrDefault(p => p.PriceListId == defaultList.Id)?.MinPrice;
        var minPrice = storedMin <= salePrice ? storedMin : null;

        inputs.Add(new ProductPriceInput(defaultList.Id, salePrice, minPrice));

        return Result<IReadOnlyList<ProductPriceInput>>.Success(inputs);
    }

    /// <summary>
    /// Request-shape checks for the prices of a product command. Missing prices and empty sale
    /// prices are left to <see cref="Resolve"/>, which can name the list in its error.
    /// </summary>
    public static void ValidateShape<T>(IReadOnlyList<ProductPriceRequest>? prices, ValidationContext<T> context)
    {
        if (prices is null)
            return;

        if (prices.GroupBy(p => p.PriceListId).Any(g => g.Count() > 1))
            context.AddFailure("Prices", "A product can have only one price per price list.");

        foreach (var price in prices)
        {
            if (price.PriceListId == Guid.Empty)
                context.AddFailure("Prices", "Price list is required for every price.");

            if (price.SalePrice is < 0)
                context.AddFailure("Prices", "Sale price must be zero or greater.");

            if (price.MinPrice is < 0)
                context.AddFailure("Prices", "Minimum price must be zero or greater.");
            else if (price.SalePrice.HasValue && price.MinPrice > price.SalePrice)
                context.AddFailure("Prices", "Minimum price must not exceed the sale price.");
        }
    }
}
