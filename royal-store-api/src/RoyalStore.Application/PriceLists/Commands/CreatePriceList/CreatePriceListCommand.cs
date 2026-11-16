using MediatR;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.CreatePriceList;

/// <param name="BasePriceListId">Optional percentage rule (062): the list this one is calculated
/// from. Sent together with <paramref name="DiscountPercentage"/> or not at all.</param>
/// <param name="DiscountPercentage">Negative = discount, positive = surcharge.</param>
public record CreatePriceListCommand(
    string Name,
    Guid? BasePriceListId = null,
    decimal? DiscountPercentage = null) : IRequest<Result<PriceListResponse>>;

