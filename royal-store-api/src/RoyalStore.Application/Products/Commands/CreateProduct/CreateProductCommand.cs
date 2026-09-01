using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
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
    List<Guid>? TagIds,
    ItemType Type = ItemType.Product,
    string? Nbs = null,
    string? IssCode = null,
    string? DefaultCfop = null,
    string? IcmsCst = null,
    string? PisCofinsCst = null,
    string? IvaCst = null,
    bool IsSelectiveTax = false) : IRequest<Result<ProductResponse>>;

public record CreateVariantRequest(
    string? Size,
    string? Color,
    string? Sku,
    string? Barcode,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked,
    decimal? PromotionalPrice = null,
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? Width = null,
    decimal? Height = null,
    decimal? Length = null);
