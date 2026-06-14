using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.DisableProduct;

public record DisableProductCommand(Guid Id) : IRequest<Result>;
