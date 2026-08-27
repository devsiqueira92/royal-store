using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string ReferenceCode,
    string? Description,
    string UnitOfMeasure,
    string? Ncm,
    string? Cest,
    int Origin,
    List<CreateVariantRequest>? Variants,
    List<Guid>? TagIds) : IRequest<Result<ProductResponse>>;

public record CreateVariantRequest(
    string? Size,
    string? Color,
    string? Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked);
