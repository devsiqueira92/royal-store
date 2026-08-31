using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetPosProducts;

public record GetPosProductsQuery(Guid? CategoryId)
    : IRequest<Result<IReadOnlyList<PosProductDto>>>;
