using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

public record ProductResponse(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    string CategoryName,
    string Name,
    string? Description,
    string? Sku,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked,
    bool IsActive,
    string? ImageUrl,
    string UnitOfMeasure,
    string? Barcode,
    string? Ncm,
    string? Cest,
    int Origin)
{
    public static ProductResponse From(Product product) =>
        new(product.Id, product.TenantId, product.CategoryId, product.Category.Name, product.Name,
            product.Description, product.Sku, product.Price, product.CostPrice, product.StockQuantity,
            product.MinStockQuantity, product.IsStockTracked, product.IsActive,
            product.ImageUrl, product.UnitOfMeasure, product.Barcode, product.Ncm,
            product.Cest, product.Origin);
}
