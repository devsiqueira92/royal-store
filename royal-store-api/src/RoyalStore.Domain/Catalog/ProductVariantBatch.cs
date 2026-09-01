namespace RoyalStore.Domain.Catalog;

public sealed record NewVariantRequest(
    string? Size,
    string? Color,
    string? Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked,
    decimal? PromotionalPrice = null,
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? Width = null,
    decimal? Height = null,
    decimal? Length = null);

public sealed record VariantUpdateRequest(
    Guid VariantId,
    string? Size,
    string? Color,
    string? Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked,
    decimal? PromotionalPrice = null,
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? Width = null,
    decimal? Height = null,
    decimal? Length = null);
