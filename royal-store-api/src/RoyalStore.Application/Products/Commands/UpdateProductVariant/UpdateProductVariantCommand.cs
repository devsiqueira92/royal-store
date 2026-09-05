using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.UpdateProductVariant;

public record UpdateProductVariantCommand(
    Guid ProductId,
    Guid VariantId,
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? Price,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity) : IRequest<Result<ProductVariantResponse>>;
