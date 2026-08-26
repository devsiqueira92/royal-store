using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    string UnitOfMeasure,
    string? Ncm,
    string? Cest,
    int Origin) : IRequest<Result<ProductResponse>>;
