using FluentValidation;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products;

/// <summary>
/// Turns what a product request carries into the price set handed to the <see cref="Product"/>
/// aggregate, and holds the request-shape rules shared by the create and edit validators.
/// <para>
/// A request states its prices in <c>Prices</c>. <c>BasePrice</c> is the pre-price-list field and is
/// honoured only when <c>Prices</c> is absent, as shorthand for "one price, on the default list" —
/// it keeps a client that has not been updated yet working, and goes away with the legacy column
/// (061 research Decision 8). Every business rule about prices lives in the aggregate, not here.
/// </para>
/// </summary>
public static class ProductPriceInputs
{
    private static readonly Error DefaultListMissing = Error.Conflict(
        "pricelist.default_missing",
        "This store has no default price list. Create or choose one before saving products.");

    public static Result<IReadOnlyList<ProductPriceInput>> ForCreate(
        IReadOnlyList<ProductPriceInput>? prices, decimal? basePrice, IReadOnlyList<PriceList> tenantLists)
    {
        if (prices is not null)
            return Result<IReadOnlyList<ProductPriceInput>>.Success(prices);

        var defaultList = tenantLists.FirstOrDefault(l => l.IsDefault);
        if (defaultList is null)
            return DefaultListMissing;

        return Result<IReadOnlyList<ProductPriceInput>>.Success(
            [new ProductPriceInput(defaultList.Id, basePrice ?? 0m, null)]);
    }

    public static Result<IReadOnlyList<ProductPriceInput>> ForEdit(
        IReadOnlyList<ProductPriceInput>? prices, decimal? basePrice,
        Product product, IReadOnlyList<PriceList> tenantLists)
    {
        if (prices is not null)
            return Result<IReadOnlyList<ProductPriceInput>>.Success(prices);

        var defaultList = tenantLists.FirstOrDefault(l => l.IsDefault);
        if (defaultList is null)
            return DefaultListMissing;

        // Legacy shorthand: only the default-list price changes. Every other stored price is
        // resubmitted as it is, so the aggregate's replace semantics leave it untouched.
        var inputs = product.Prices
            .Where(p => p.PriceListId != defaultList.Id)
            .Select(p => new ProductPriceInput(p.PriceListId, p.SalePrice, p.MinPrice))
            .ToList();

        var salePrice = basePrice ?? 0m;

        // A legacy client cannot see or edit the discount floor. Keep it while it still fits under
        // the new price; drop it otherwise, rather than reject a save the client could never fix.
        var storedMin = product.Prices.FirstOrDefault(p => p.PriceListId == defaultList.Id)?.MinPrice;
        var minPrice = storedMin <= salePrice ? storedMin : null;

        inputs.Add(new ProductPriceInput(defaultList.Id, salePrice, minPrice));

        return Result<IReadOnlyList<ProductPriceInput>>.Success(inputs);
    }

    /// <summary>Request-shape checks for the price fields of a product command.</summary>
    public static void ValidateShape<T>(
        IReadOnlyList<ProductPriceInput>? prices, decimal? basePrice, ValidationContext<T> context)
    {
        if (prices is null)
        {
            if (!basePrice.HasValue)
                context.AddFailure("Prices", "At least the price on the default price list is required.");
            else if (basePrice.Value < 0)
                context.AddFailure("BasePrice", "Base price must be zero or greater.");

            return;
        }

        if (prices.Count == 0)
        {
            context.AddFailure("Prices", "At least the price on the default price list is required.");
            return;
        }

        if (prices.GroupBy(p => p.PriceListId).Any(g => g.Count() > 1))
            context.AddFailure("Prices", "A product can have only one price per price list.");

        foreach (var price in prices)
        {
            if (price.PriceListId == Guid.Empty)
                context.AddFailure("Prices", "Price list is required for every price.");

            if (price.SalePrice < 0)
                context.AddFailure("Prices", "Sale price must be zero or greater.");

            if (price.MinPrice is < 0)
                context.AddFailure("Prices", "Minimum price must be zero or greater.");
            else if (price.MinPrice > price.SalePrice)
                context.AddFailure("Prices", "Minimum price must not exceed the sale price.");
        }
    }
}
