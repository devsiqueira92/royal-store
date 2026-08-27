using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.UpdateProduct;

public sealed record VariantInput(
    string? Size,
    string? Color,
    string? Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked);

public sealed record VariantUpdateInput(
    Guid VariantId,
    string? Size,
    string? Color,
    string? Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked);

public record UpdateProductCommand(
    Guid Id,
    int Version,
    Guid CategoryId,
    string Name,
    string ReferenceCode,
    string? Description,
    string UnitOfMeasure,
    string? Ncm,
    string? Cest,
    int Origin,
    IReadOnlyList<VariantInput> VariantsToAdd,
    IReadOnlyList<VariantUpdateInput> VariantsToUpdate)
    : IRequest<Result<ProductResponse>>
{
    public IReadOnlyList<VariantInput> VariantsToAdd { get; init; } = VariantsToAdd ?? [];
    public IReadOnlyList<VariantUpdateInput> VariantsToUpdate { get; init; } = VariantsToUpdate ?? [];
}
