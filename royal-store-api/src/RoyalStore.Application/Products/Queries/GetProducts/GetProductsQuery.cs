using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Queries.GetProducts;

public record GetProductsQuery(bool? IsActive, Guid? CategoryId)
    : IRequest<Result<IReadOnlyList<ProductResponse>>>;
