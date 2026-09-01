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
    decimal Price,
    string? CoverImage,
    string? VariantSize,
    bool IsStockTracked,
    decimal StockQuantity,
    bool SoldOut,
    int Origin,
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
    public static PosProductDto From(Product product, ProductVariant variant, IbptTax? ibptTax) =>
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
            variant.IsStockTracked && variant.StockQuantity == 0m,
            product.Origin,
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
