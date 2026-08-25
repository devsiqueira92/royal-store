using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

public record ProductResponse(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    string CategoryName,
    string? CategoryColor,
    string Name,
    string? Description,
    string? Sku,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked,
    bool IsActive,
    string UnitOfMeasure,
    string? Barcode,
    string? Ncm,
    string? Cest,
    int Origin,
    IReadOnlyList<string> Sizes,
    string? CoverPhotoUrl,
    IReadOnlyList<ProductPhotoResponse> Photos)
{
    public static ProductResponse From(Product product) =>
        new(product.Id, product.TenantId, product.CategoryId, product.Category.Name, product.Category.Color,
            product.Name, product.Description, product.Sku, product.Price, product.CostPrice, product.StockQuantity,
            product.MinStockQuantity, product.IsStockTracked, product.IsActive,
            product.UnitOfMeasure, product.Barcode, product.Ncm, product.Cest, product.Origin,
            product.Sizes.Select(s => s.Size.ToString()).ToList(),
            product.Photos.FirstOrDefault(p => p.IsCover)?.Url,
            product.Photos.Select(p => new ProductPhotoResponse(p.Id, p.Url, p.IsCover)).ToList());
}
