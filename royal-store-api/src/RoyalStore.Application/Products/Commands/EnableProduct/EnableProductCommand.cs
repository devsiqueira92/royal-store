using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.EnableProduct;

public record EnableProductCommand(Guid Id) : IRequest<Result>;
