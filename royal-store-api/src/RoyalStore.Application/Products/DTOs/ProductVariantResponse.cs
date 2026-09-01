using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

public record ProductVariantResponse(
    Guid Id,
    string? Size,
    string? Color,
    string? Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked,
    decimal? PromotionalPrice,
    decimal? NetWeight,
    decimal? GrossWeight,
    decimal? Width,
    decimal? Height,
    decimal? Length)
{
    public static ProductVariantResponse From(ProductVariant v) =>
        new(v.Id, v.Size, v.Color, v.Sku, v.Barcode, v.Price, v.CostPrice,
            v.StockQuantity, v.MinStockQuantity, v.IsStockTracked,
            v.PromotionalPrice, v.NetWeight, v.GrossWeight, v.Width, v.Height, v.Length);
}
