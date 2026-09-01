using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

public record ProductResponse(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    string CategoryName,
    string? CategoryColor,
    string Name,
    string ReferenceCode,
    string? Description,
    bool IsActive,
    string UnitOfMeasure,
    string? Ncm,
    string? Cest,
    int Origin,
    int Type,
    string? Nbs,
    string? IssCode,
    string? DefaultCfop,
    string? IcmsCst,
    string? PisCofinsCst,
    string? IvaCst,
    bool IsSelectiveTax,
    int Version,
    string? CoverPhotoUrl,
    IReadOnlyList<ProductPhotoResponse> Photos,
    IReadOnlyList<ProductVariantResponse> Variants,
    IReadOnlyList<ProductTagResponse> Tags)
{
    public static ProductResponse From(Product product) =>
        new(product.Id, product.TenantId, product.CategoryId,
            product.Category.Name, product.Category.Color,
            product.Name, product.ReferenceCode, product.Description, product.IsActive,
            product.UnitOfMeasure, product.Ncm, product.Cest, (int)product.Origin, (int)product.Type,
            product.Nbs, product.IssCode, product.DefaultCfop, product.IcmsCst,
            product.PisCofinsCst, product.IvaCst, product.IsSelectiveTax, product.Version,
            product.Photos.FirstOrDefault(p => p.IsCover)?.Url,
            product.Photos.Select(p => new ProductPhotoResponse(p.Id, p.Url, p.IsCover)).ToList(),
            product.Variants.Select(ProductVariantResponse.From).ToList(),
            product.Tags.Select(ProductTagResponse.From).ToList());
}
