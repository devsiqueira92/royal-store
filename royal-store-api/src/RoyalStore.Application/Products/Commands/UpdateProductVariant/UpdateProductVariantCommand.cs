using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.UpdateProductVariant;

public record UpdateProductVariantCommand(
    Guid ProductId,
    Guid VariantId,
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
    decimal? Length = null) : IRequest<Result<ProductVariantResponse>>;
