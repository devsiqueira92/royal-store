using MediatR;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.SetDefaultPriceList;

/// <summary>
/// Makes a price list the store's default. Sales charge the default-list price, so this re-prices
/// the whole store: it is refused while any product lacks a price on the target list (061 FR-005).
/// </summary>
public record SetDefaultPriceListCommand(Guid Id) : IRequest<Result<PriceListResponse>>;
