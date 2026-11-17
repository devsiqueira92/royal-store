using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string Sku,
    bool StockTrack,
    // Deprecated (061): read only when Prices is absent, as one price on the default list.
    decimal? BasePrice,
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
    Guid? SupplierId = null,
    // One sale price per price list; the default list is mandatory (061 FR-010).
    IReadOnlyList<ProductPriceInput>? Prices = null,
    // The product's cost breakdown; every line is new (063 FR-008).
    IReadOnlyList<ProductCostItemInput>? CostItems = null) : IRequest<Result<ProductResponse>>;

public record CreateVariantRequest(
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity);
