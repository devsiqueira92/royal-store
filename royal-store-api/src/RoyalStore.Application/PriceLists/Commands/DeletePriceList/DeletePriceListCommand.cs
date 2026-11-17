using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.DeletePriceList;

/// <summary>Deletes a price list that is neither the default nor priced on any product (061 FR-006).</summary>
public record DeletePriceListCommand(Guid Id) : IRequest<Result>;
