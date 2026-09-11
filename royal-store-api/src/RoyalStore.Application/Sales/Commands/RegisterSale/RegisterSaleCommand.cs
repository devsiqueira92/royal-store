using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Commands.RegisterSale;

/// <summary>
/// Records one completed point-of-sale checkout. The terminal states what was sold, who's buying
/// (if anyone), how it's being paid and fulfilled; the backend is the sole source of truth for every
/// discount, tax, and total figure — nothing about pricing is ever taken from the request.
/// </summary>
public record RegisterSaleCommand(
    IReadOnlyList<RegisterSaleItem> Items,
    RegisterSaleCheckout Checkout) : IRequest<Result<RegisterSaleResponse>>;
