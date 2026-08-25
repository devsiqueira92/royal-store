using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    Guid CategoryId,
    string Name,
    string? Description,
    string? Sku,
    decimal Price,
    decimal? CostPrice,
    decimal StockQuantity,
    decimal? MinStockQuantity,
    bool IsStockTracked,
    string UnitOfMeasure,
    string? Barcode,
    string? Ncm,
    string? Cest,
    int Origin) : IRequest<Result<ProductResponse>>;
