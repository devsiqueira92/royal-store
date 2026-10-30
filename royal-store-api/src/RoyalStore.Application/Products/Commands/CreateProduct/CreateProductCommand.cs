using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
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
    List<CreateVariantRequest>? Variants,
    decimal? NetWeight = null,
    decimal? GrossWeight = null,
    decimal? Width = null,
    decimal? Height = null,
    decimal? Length = null,
    string? DefaultCfop = null,
    string? IcmsCst = null,
    string? PisCofinsCst = null,
    string? IvaCst = null,
    bool? IsSelectiveTax = null,
    Guid? SupplierId = null) : IRequest<Result<ProductResponse>>;

public record CreateVariantRequest(
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? Price,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity);
