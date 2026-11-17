using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.SetPriceListRule;

/// <summary>
/// Gives a price list a percentage rule, or changes the one it has (062): "this list is
/// <paramref name="BasePriceListId"/> plus <paramref name="DiscountPercentage"/> percent".
/// The rule only feeds suggestions — no stored product price is touched.
/// </summary>
/// <param name="DiscountPercentage">Negative = discount, positive = surcharge.</param>
public record SetPriceListRuleCommand(
    Guid Id,
    Guid? BasePriceListId,
    decimal? DiscountPercentage) : IRequest<Result<PriceListResponse>>;
