using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    string? Sku,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked,
    string? ImageUrl,
    string UnitOfMeasure,
    string? Barcode,
    string? Ncm,
    string? Cest,
    int Origin) : IRequest<Result<ProductResponse>>;
