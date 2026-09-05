namespace RoyalStore.Domain.Catalog;

public sealed record NewVariantRequest(
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? Price,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity);

public sealed record VariantUpdateRequest(
    Guid VariantId,
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? Price,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity);
