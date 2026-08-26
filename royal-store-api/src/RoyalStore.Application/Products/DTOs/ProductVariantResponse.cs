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
    bool IsStockTracked)
{
    public static ProductVariantResponse From(ProductVariant v) =>
        new(v.Id, v.Size, v.Color, v.Sku, v.Barcode, v.Price, v.CostPrice,
            v.StockQuantity, v.MinStockQuantity, v.IsStockTracked);
}
