using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Queries.GetProducts;

public record GetProductsQuery(
    bool? IsActive,
    Guid? CategoryId,
    StockStatus? StockStatus = null,
    Guid? SupplierId = null,
    string? Search = null)
    : IRequest<Result<IReadOnlyList<ProductResponse>>>;
