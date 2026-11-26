using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetSalePriceListOptions;

/// <summary>
/// The price lists a sale can be charged on — the store's active lists, default first, then by
/// name — each with the cart priced on it. The chosen list's id goes back as the checkout's
/// <c>priceListId</c>. Open to whoever can sell, unlike <c>GET api/price-lists</c>.
/// </summary>
public sealed record GetSalePriceListOptionsQuery(IReadOnlyList<SalePricingItem> Items)
    : IRequest<Result<IReadOnlyList<SalePriceListOptionResponse>>>;
