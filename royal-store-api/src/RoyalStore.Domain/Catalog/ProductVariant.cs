using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class ProductVariant : Entity
{
    public Guid ProductId { get; private set; }
    public Guid TenantId { get; private set; }
    public string? Size { get; private set; }
    public string? Color { get; private set; }
    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CostPrice { get; private set; }
    public decimal StockQuantity { get; private set; }
    public decimal? MinStockQuantity { get; private set; }
    public bool IsStockTracked { get; private set; }
    public decimal? PromotionalPrice { get; private set; }
    public decimal? NetWeight { get; private set; }
    public decimal? GrossWeight { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; }
    public decimal? Length { get; private set; }

    private ProductVariant() : base(Guid.NewGuid()) { }

    internal ProductVariant(Guid productId, Guid tenantId, string? size, string? color,
        string? sku, string? barcode, decimal price, decimal? costPrice,
        decimal stockQuantity, decimal? minStockQuantity, bool isStockTracked,
        decimal? promotionalPrice, decimal? netWeight, decimal? grossWeight,
        decimal? width, decimal? height, decimal? length)
        : base(Guid.NewGuid())
    {
        ProductId = productId;
        TenantId = tenantId;
        Size = size?.Trim();
        Color = color?.Trim();
        Sku = sku?.Trim();
        Barcode = barcode?.Trim();
        Price = price;
        CostPrice = costPrice;
        StockQuantity = stockQuantity;
        MinStockQuantity = minStockQuantity;
        IsStockTracked = isStockTracked;
        PromotionalPrice = promotionalPrice;
        NetWeight = netWeight;
        GrossWeight = grossWeight;
        Width = width;
        Height = height;
        Length = length;
    }

    public Result Update(string? size, string? color, string? sku, string? barcode,
        decimal price, decimal? costPrice, decimal stockQuantity, decimal? minStockQuantity, bool isStockTracked,
        decimal? promotionalPrice, decimal? netWeight, decimal? grossWeight,
        decimal? width, decimal? height, decimal? length)
    {
        if (price < 0)
            return Result.Failure(Error.Validation("variant.price_negative", "Price must be zero or greater."));

        if (costPrice.HasValue && costPrice.Value < 0)
            return Result.Failure(Error.Validation("variant.cost_price_negative", "Cost price must be zero or greater."));

        if (stockQuantity < 0)
            return Result.Failure(Error.Validation("variant.stock_negative", "Stock quantity must be zero or greater."));

        if (promotionalPrice is < 0)
            return Result.Failure(Error.Validation("variant.promotional_price_negative", "Promotional price must be zero or greater."));

        if (promotionalPrice.HasValue && promotionalPrice.Value > price)
            return Result.Failure(Error.Validation("variant.promotional_price_above_price", "Promotional price must not exceed the regular price."));

        if (HasNegativeMeasure(netWeight, grossWeight, width, height, length))
            return Result.Failure(Error.Validation("variant.measure_negative", "Weight and dimension values must be zero or greater."));

        Size = size?.Trim();
        Color = color?.Trim();
        Sku = sku?.Trim();
        Barcode = barcode?.Trim();
        Price = price;
        CostPrice = costPrice;
        StockQuantity = stockQuantity;
        MinStockQuantity = minStockQuantity;
        IsStockTracked = isStockTracked;
        PromotionalPrice = promotionalPrice;
        NetWeight = netWeight;
        GrossWeight = grossWeight;
        Width = width;
        Height = height;
        Length = length;

        return Result.Success();
    }

    private static bool HasNegativeMeasure(params decimal?[] values)
    {
        foreach (var value in values)
        {
            if (value is < 0)
                return true;
        }

        return false;
    }
}
