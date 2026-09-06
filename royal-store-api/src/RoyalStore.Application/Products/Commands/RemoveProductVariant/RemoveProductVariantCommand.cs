using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.RemoveProductVariant;

public record RemoveProductVariantCommand(Guid VariantId) : IRequest<Result>;
