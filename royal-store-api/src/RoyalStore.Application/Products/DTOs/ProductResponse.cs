using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

public record ProductResponse(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    string CategoryName,
    string? CategoryColor,
    string Name,
    string Sku,
    string? Description,
    bool IsActive,
    string UnitOfMeasure,
    int Type,
    bool StockTrack,
    decimal BasePrice,
    decimal BaseCostPrice,
    string? Ncm,
    string? Cest,
    int Origin,
    string? Nbs,
    string? IssCode,
    ProductMeasureResponse Measure,
    Guid TaxProfileTemplateId,
    bool OverrideTaxProfile,
    string? DefaultCfop,
    string? IcmsCst,
    string? PisCofinsCst,
    string? IvaCst,
    bool? IsSelectiveTax,
    int Version,
    string? CoverPhotoUrl,
    IReadOnlyList<ProductPhotoResponse> Photos,
    IReadOnlyList<ProductVariantResponse> Variants)
{
    public static ProductResponse From(Product product) =>
        new(product.Id, product.TenantId, product.CategoryId,
            product.Category.Name, product.Category.Color,
            product.Name, product.Sku, product.Description, product.IsActive,
            product.UnitOfMeasure, (int)product.Type, product.StockTrack,
            product.BasePrice, product.BaseCostPrice,
            product.Ncm, product.Cest, (int)product.Origin,
            product.Nbs, product.IssCode,
            ProductMeasureResponse.From(product.Measure),
            product.TaxProfileTemplateId, product.OverrideTaxProfile,
            product.DefaultCfop, product.IcmsCst, product.PisCofinsCst, product.IvaCst, product.IsSelectiveTax,
            product.Version,
            product.Photos.FirstOrDefault(p => p.IsCover)?.Url,
            product.Photos.Select(p => new ProductPhotoResponse(p.Id, p.Url, p.IsCover)).ToList(),
            product.Variants.Select(ProductVariantResponse.From).ToList());
}
