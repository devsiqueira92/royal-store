using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetSalesList;

public record GetSalesListQuery(
    string? SellerName = null,
    string? CustomerName = null,
    DateOnly? SoldFrom = null,
    DateOnly? SoldTo = null,
    string? FulfillmentMode = null) : IRequest<Result<IReadOnlyList<SaleListItemResponse>>>;
