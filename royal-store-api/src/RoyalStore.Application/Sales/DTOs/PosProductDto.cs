using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Fiscal;

namespace RoyalStore.Application.Sales.DTOs;

public record PosProductDto(
    Guid ProductId,
    Guid VariantId,
    string ProductName,
    string Category,
    string? CategoryColor,
    string? VariantColor,
    string? Barcode,
    string Sku,
    decimal Price,
    string? CoverImage,
    string? VariantSize,
    bool IsStockTracked,
    decimal StockQuantity,
    bool SoldOut,
    string Origin,
    int Type,
    bool IsSelectiveTax,
    string? IvaCst,
    string? IcmsCst,
    string? PisCofinsCst,
    string? DefaultCfop,
    decimal? NationalRate,
    decimal? FederalRate,
    decimal? ImportedFederalRate,
    decimal? StateRate,
    decimal? MunicipalRate,
    DateOnly? EffectiveStart,
    DateOnly? EffectiveEnd,
    string? Source,
    string? Key)
{
    public static PosProductDto From(Product product, ProductVariant variant, IbptTax? ibptTax)
    {
        // The fiscal classification in force follows the product's OverrideTaxProfile flag: when it
        // is false the product's own CST/CFOP columns are null and the values live on its tax
        // profile template. Resolving it here keeps the catalog and a stored sale line in agreement.
        var fiscal = product.ResolveFiscalProfile();

        return new PosProductDto(
            product.Id,
            variant.Id,
            product.Name,
            product.Category.Name,
            product.Category.Color,
            variant.Color,
            variant.Barcode,
            variant.Sku ?? product.Sku,
            variant.EffectivePrice(product),
            product.Photos.FirstOrDefault(p => p.IsCover)?.Url,
            variant.Size,
            product.StockTrack,
            variant.StockQuantity ?? 0m,
            product.StockTrack && (variant.StockQuantity ?? 0m) == 0m,
            product.Origin,
            (int)product.Type,
            fiscal.IsSuccess && fiscal.Value.IsSelectiveTax,
            fiscal.IsSuccess ? fiscal.Value.IvaCst : null,
            fiscal.IsSuccess ? fiscal.Value.IcmsCst : null,
            fiscal.IsSuccess ? fiscal.Value.PisCofinsCst : null,
            fiscal.IsSuccess ? fiscal.Value.Cfop : null,
            ibptTax?.NationalRate,
            ibptTax?.FederalRate,
            ibptTax?.ImportedFederalRate,
            ibptTax?.StateRate,
            ibptTax?.MunicipalRate,
            ibptTax?.EffectiveStart,
            ibptTax?.EffectiveEnd,
            ibptTax?.Source,
            ibptTax?.Key);
    }
}
