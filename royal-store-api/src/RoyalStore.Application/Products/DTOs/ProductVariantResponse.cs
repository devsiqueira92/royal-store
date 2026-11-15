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
    /// <summary>
    /// <c>Price</c> is a service option's own price. For a product variant it is always null: the
    /// variant is priced by its product's price lists, and the value still stored on the row is a
    /// leftover that is no longer read (061 FR-026, FR-027).
    /// </summary>
    public static ProductVariantResponse From(ProductVariant v, Product owner) =>
        new(v.Id, v.Size, v.Color, v.OptionName, v.Sku, v.Barcode,
            owner.Type == ItemType.Service ? v.Price : null, v.CostPrice,
            v.StockQuantity, v.MinStockQuantity);
}
