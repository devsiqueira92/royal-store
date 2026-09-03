using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

public record ProductVariantResponse(
    Guid Id,
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? Price,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity)
{
    public static ProductVariantResponse From(ProductVariant v) =>
        new(v.Id, v.Size, v.Color, v.OptionName, v.Sku, v.Barcode, v.Price, v.CostPrice,
            v.StockQuantity, v.MinStockQuantity);
}
