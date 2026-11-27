using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

/// <summary>A variant carries no price: it is priced by its product's price lists.</summary>
public record ProductVariantResponse(
    Guid Id,
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? StockQuantity,
    decimal? MinStockQuantity)
{
    public static ProductVariantResponse From(ProductVariant v, Product owner) =>
        new(v.Id, v.Size, v.Color, v.OptionName, v.Sku, v.Barcode,
            v.StockQuantity, v.MinStockQuantity);
}
