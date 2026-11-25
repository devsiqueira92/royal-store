using MediatR;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.UpdatePriceList;

/// <summary>
/// Saves a price list's editable data in one request: its name and its percentage rule (062).
/// Any list can be renamed, the default one included. The rule is the whole desired state:
/// base list and percentage together set (or change) the rule, both null remove it.
/// No stored product price is touched.
/// </summary>
/// <param name="BasePriceListId">The list this one is calculated from; null with
/// <paramref name="DiscountPercentage"/> null = priced by hand.</param>
/// <param name="DiscountPercentage">Negative = discount, positive = surcharge.</param>
public record UpdatePriceListCommand(
    Guid Id,
    string Name,
    Guid? BasePriceListId = null,
    decimal? DiscountPercentage = null) : IRequest<Result<PriceListResponse>>;
