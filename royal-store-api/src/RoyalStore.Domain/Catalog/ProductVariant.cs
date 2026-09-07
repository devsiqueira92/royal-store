using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class ProductVariant : Entity
{
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    public string? Size { get; private set; }
    public string? Color { get; private set; }
    public string? OptionName { get; private set; }
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }
    public decimal? Price { get; private set; }
    public decimal? CostPrice { get; private set; }
    public decimal? StockQuantity { get; private set; }
    public decimal? MinStockQuantity { get; private set; }

    private ProductVariant() : base(Guid.NewGuid()) { }

    internal ProductVariant(Guid productId, Guid tenantId, string? size, string? color, string? optionName,
        string? sku, string? barcode, decimal? price, decimal? costPrice,
        decimal? stockQuantity, decimal? minStockQuantity)
        : base(Guid.NewGuid())
    {
        ProductId = productId;
        TenantId = tenantId;
        Size = size?.Trim();
        Color = color?.Trim();
        OptionName = optionName?.Trim();
        Sku = sku?.Trim();
        Barcode = barcode?.Trim();
        Price = price;
        CostPrice = costPrice;
        StockQuantity = stockQuantity;
        MinStockQuantity = minStockQuantity;
    }

    internal static Result Validate(
        bool ownerStockTrack, decimal ownerBasePrice, decimal ownerBaseCostPrice,
        string? sku, string? barcode, decimal? price, decimal? costPrice,
        decimal? stockQuantity, decimal? minStockQuantity)
    {
        if (ownerStockTrack)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return Result.Failure(Error.Validation("variant.sku_required", "SKU is required when stock is tracked."));

            if (string.IsNullOrWhiteSpace(barcode))
                return Result.Failure(Error.Validation("variant.barcode_required", "Barcode is required when stock is tracked."));

            if (!stockQuantity.HasValue)
                return Result.Failure(Error.Validation("variant.stock_quantity_required", "Stock quantity is required when stock is tracked."));

            if (!minStockQuantity.HasValue)
                return Result.Failure(Error.Validation("variant.min_stock_quantity_required", "Minimum stock quantity is required when stock is tracked."));
        }

        if (stockQuantity is < 0)
            return Result.Failure(Error.Validation("variant.stock_negative", "Stock quantity must be zero or greater."));

        if (minStockQuantity is < 0)
            return Result.Failure(Error.Validation("variant.min_stock_negative", "Minimum stock quantity must be zero or greater."));

        if (price.HasValue)
        {
            if (price.Value < 0)
                return Result.Failure(Error.Validation("variant.price_negative", "Price must be zero or greater."));

            if (price.Value < ownerBasePrice)
                return Result.Failure(Error.Validation("variant.price_not_above_base", "Price must be greater than the product's base price."));
        }

        if (costPrice.HasValue)
        {
            if (costPrice.Value < 0)
                return Result.Failure(Error.Validation("variant.cost_price_negative", "Cost price must be zero or greater."));

            if (costPrice.Value < ownerBaseCostPrice)
                return Result.Failure(Error.Validation("variant.cost_price_not_above_base", "Cost price must be greater than the product's base cost price."));
        }

        return Result.Success();
    }

    public Result Update(string? size, string? color, string? optionName, string? sku, string? barcode,
        decimal? price, decimal? costPrice, decimal? stockQuantity, decimal? minStockQuantity,
        bool ownerStockTrack, decimal ownerBasePrice, decimal ownerBaseCostPrice)
    {
        var validation = Validate(ownerStockTrack, ownerBasePrice, ownerBaseCostPrice,
            sku, barcode, price, costPrice, stockQuantity, minStockQuantity);
        if (!validation.IsSuccess)
            return validation;

        Size = size?.Trim();
        Color = color?.Trim();
        OptionName = optionName?.Trim();
        Sku = sku?.Trim();
        Barcode = barcode?.Trim();
        Price = price;
        CostPrice = costPrice;
        StockQuantity = stockQuantity;
        MinStockQuantity = minStockQuantity;

        return Result.Success();
    }

    /// <summary>
    /// Human-readable composition of the variant's distinguishing attributes, used on receipts and
    /// snapshotted onto a sale line. Falls back to the product name when the variant has none.
    /// </summary>
    public string BuildLabel(Product owner)
    {
        var parts = new[] { Size, Color, OptionName }
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim());

        var label = string.Join(" / ", parts);

        if (string.IsNullOrWhiteSpace(label))
            label = owner.Name;

        return label.Length > 200 ? label[..200] : label;
    }

    /// <summary>The price in force: the variant's own when set, otherwise the product's base price.</summary>
    public decimal EffectivePrice(Product owner) => Price ?? owner.BasePrice;
}
