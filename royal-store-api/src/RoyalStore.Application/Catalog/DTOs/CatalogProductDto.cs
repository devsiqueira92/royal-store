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
    public static CatalogProductDto From(Product p) =>
        new(p.Id, p.CategoryId, p.Name, p.Sku, p.Barcode, p.Price,
            p.UnitOfMeasure, p.Ncm, p.Cest, p.Origin, p.IsActive, p.UpdatedAt);
}
