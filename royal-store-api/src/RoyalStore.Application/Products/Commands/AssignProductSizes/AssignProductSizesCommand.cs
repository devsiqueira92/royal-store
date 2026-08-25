using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.AssignProductSizes;

public record AssignProductSizesCommand(Guid ProductId, List<string> Sizes)
    : IRequest<Result<ProductResponse>>;
