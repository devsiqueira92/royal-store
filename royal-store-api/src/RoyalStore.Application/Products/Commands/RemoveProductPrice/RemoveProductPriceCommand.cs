using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.RemoveProductPrice;

/// <summary>
/// Removes a product's price on one price list. Editing a product leaves prices on inactive lists
/// alone, so this is the explicit way to drop one (061 FR-012).
/// </summary>
public record RemoveProductPriceCommand(Guid ProductId, Guid PriceListId) : IRequest<Result>;
