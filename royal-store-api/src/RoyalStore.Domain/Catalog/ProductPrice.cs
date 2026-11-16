using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

/// <summary>One price submitted for a product: which list, how much, and the optional discount floor.</summary>
public sealed record ProductPriceInput(Guid PriceListId, decimal SalePrice, decimal? MinPrice);

/// <summary>
/// What <see cref="Product.SetPrices"/> added to and removed from the aggregate, so the repository
/// can register those children explicitly — the same reason <c>AddVariantAsync</c> exists: a child
/// with a client-generated key discovered on a tracked graph is mistaken for an existing row.
/// </summary>
public sealed record ProductPriceChanges(
    IReadOnlyList<ProductPrice> Added,
    IReadOnlyList<ProductPrice> Removed)
{
    public static readonly ProductPriceChanges None = new([], []);
}

/// <summary>
/// The sale price of one product on one <see cref="PriceList"/>. Owned by the <see cref="Product"/>
/// aggregate: only the product creates, changes or removes it.
/// </summary>
public sealed class ProductPrice : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid PriceListId { get; private set; }
    public PriceList PriceList { get; private set; } = null!;
    public decimal SalePrice { get; private set; }

    /// <summary>Floor below which the item should not be discounted at the point of sale. Stored
    /// only — nothing enforces it yet (061 FR-028).</summary>
    public decimal? MinPrice { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private ProductPrice() : base(Guid.NewGuid()) { }

    internal ProductPrice(Guid tenantId, Guid productId, Guid priceListId, decimal salePrice, decimal? minPrice)
        : base(Guid.NewGuid())
    {
        TenantId = tenantId;
        ProductId = productId;
        PriceListId = priceListId;
        SalePrice = salePrice;
        MinPrice = minPrice;
    }

    internal static Result Validate(decimal salePrice, decimal? minPrice)
    {
        if (salePrice < 0)
            return Result.Failure(Error.Validation("product.price_negative", "Sale price must be zero or greater."));

        if (minPrice is < 0)
            return Result.Failure(Error.Validation("product.min_price_negative", "Minimum price must be zero or greater."));

        if (minPrice > salePrice)
            return Result.Failure(Error.Validation("product.min_price_above_sale_price",
                "Minimum price must not exceed the sale price."));

        return Result.Success();
    }

    internal bool HasValues(decimal salePrice, decimal? minPrice) =>
        SalePrice == salePrice && MinPrice == minPrice;

    internal void Change(decimal salePrice, decimal? minPrice)
    {
        SalePrice = salePrice;
        MinPrice = minPrice;
    }

    /// <summary>
    /// Carries a product's former single sale price over to the store's default list (061 FR-021).
    /// Used only by the backfill of products that existed before price lists; every other price is
    /// created through <see cref="Product.SetPrices"/>.
    /// </summary>
    public static Result<ProductPrice> CreateFromLegacyPrice(
        Guid tenantId, Guid productId, Guid defaultPriceListId, decimal legacySalePrice)
    {
        if (tenantId == Guid.Empty || productId == Guid.Empty || defaultPriceListId == Guid.Empty)
            return Error.Validation("product.price_reference_required",
                "Tenant, product and price list are required.");

        var validation = Validate(legacySalePrice, null);
        if (validation.IsFailure)
            return validation.Error;

        return new ProductPrice(tenantId, productId, defaultPriceListId, legacySalePrice, null);
    }
}
