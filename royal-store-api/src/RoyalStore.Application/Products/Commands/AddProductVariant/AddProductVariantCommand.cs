using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.AddProductVariant;

public record AddProductVariantCommand(
    Guid ProductId,
    string? Size,
    string? Color,
    string? OptionName,
    string? Sku,
    string? Barcode,
    decimal? Price,
    decimal? CostPrice,
    decimal? StockQuantity,
    decimal? MinStockQuantity) : IRequest<Result<ProductVariantResponse>>;
