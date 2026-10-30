using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.EditProduct;

public sealed record VariantInput(
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? Price,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity);

public sealed record VariantUpdateInput(
    Guid VariantId,
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? Price,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity);

public sealed record MeasureInput(
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? Width = null,
    decimal? Height = null,
    decimal? Length = null);

public record EditProductCommand(
    Guid Id,
    int Version,
    Guid CategoryId,
    string Name,
    string Sku,
    bool StockTrack,
    decimal BasePrice,
    decimal BaseCostPrice,
    string? Description,
    string UnitOfMeasure,
    string? Ncm,
    string? Cest,
    string Origin,
    Guid TaxProfileTemplateId,
    bool OverrideTaxProfile,
    IReadOnlyList<VariantInput> VariantsToAdd,
    IReadOnlyList<VariantUpdateInput> VariantsToUpdate,
    MeasureInput Measure,
    string? DefaultCfop = null,
    string? IcmsCst = null,
    string? PisCofinsCst = null,
    string? IvaCst = null,
    bool? IsSelectiveTax = null,
    Guid? SupplierId = null)
    : IRequest<Result<ProductResponse>>
{
    public IReadOnlyList<VariantInput> VariantsToAdd { get; init; } = VariantsToAdd ?? [];
    public IReadOnlyList<VariantUpdateInput> VariantsToUpdate { get; init; } = VariantsToUpdate ?? [];
}
