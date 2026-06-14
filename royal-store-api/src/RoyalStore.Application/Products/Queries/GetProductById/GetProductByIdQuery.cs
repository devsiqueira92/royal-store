using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductResponse>>;
