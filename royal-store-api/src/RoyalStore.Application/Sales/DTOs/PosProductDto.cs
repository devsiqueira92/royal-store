using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Sales.DTOs;

public record PosProductDto(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string Category,
    string? CategoryColor,
    string? VariantColor,
    decimal Price,
    string? CoverImage,
    string? VariantSize,
    bool IsStockTracked,
    decimal StockQuantity,
    bool SoldOut)
{
    public static PosProductDto From(Product product, ProductVariant variant) =>
        new(
            product.Id,
            variant.Id,
            product.Name,
            product.Category.Name,
            product.Category.Color,
            variant.Color,
            variant.Price,
            product.Photos.FirstOrDefault(p => p.IsCover)?.Url,
            variant.Size,
            variant.IsStockTracked,
            variant.StockQuantity,
            variant.IsStockTracked && variant.StockQuantity == 0m);
}
