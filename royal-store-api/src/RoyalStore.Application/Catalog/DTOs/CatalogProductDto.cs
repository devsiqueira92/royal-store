using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Catalog.DTOs;

public record CatalogProductDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Sku,
    string? Barcode,
    decimal Price,
    string UnitOfMeasure,
    string? Ncm,
    string? Cest,
    int Origin,
    bool IsActive,
    DateTimeOffset UpdatedAt)
{
    public static CatalogProductDto From(Product p)
    {
        var first = p.Variants.FirstOrDefault();
        return new(p.Id, p.CategoryId, p.Name, first?.Sku, first?.Barcode,
            first?.Price ?? 0m, p.UnitOfMeasure, p.Ncm, p.Cest, (int)p.Origin, p.IsActive, p.UpdatedAt);
    }
}
