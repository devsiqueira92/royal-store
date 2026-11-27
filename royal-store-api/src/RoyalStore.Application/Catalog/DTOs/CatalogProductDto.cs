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
    string Origin,
    bool IsActive,
    DateTimeOffset UpdatedAt)
{
    /// <summary><c>Price</c> is the product's price on the default list; requires its prices loaded
    /// with their list (<c>GetForCatalogAsync</c>).</summary>
    public static CatalogProductDto From(Product p)
    {
        var first = p.Variants.FirstOrDefault();
        return new(p.Id, p.CategoryId, p.Name, first?.Sku, first?.Barcode,
            p.DefaultSalePrice ?? 0m, p.UnitOfMeasure, p.Ncm, p.Cest, p.Origin, p.IsActive, p.UpdatedAt);
    }
}
