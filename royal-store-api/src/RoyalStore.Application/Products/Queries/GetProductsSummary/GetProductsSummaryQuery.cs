using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Queries.GetProductsSummary;

public record GetProductsSummaryQuery(Guid? CategoryId, Guid? SupplierId)
    : IRequest<Result<ProductsSummaryResponse>>;
