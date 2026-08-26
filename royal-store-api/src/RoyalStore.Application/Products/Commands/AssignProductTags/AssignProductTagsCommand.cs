using MediatR;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.AssignProductTags;

public record AssignProductTagsCommand(Guid ProductId, List<Guid> TagIds) : IRequest<Result<ProductResponse>>;
