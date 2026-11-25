using MediatR;
using RoyalStore.Application.Common.Pagination;
using RoyalStore.Application.DTOs;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetSalesList;

/// <summary>The store's sales, newest first, one cursor page at a time (057, paged since 067).</summary>
public record GetSalesListQuery(
    string? SellerName = null,
    string? CustomerName = null,
    DateOnly? SoldFrom = null,
    DateOnly? SoldTo = null,
    string? FulfillmentMode = null,
    PageRequest? Page = null) : IRequest<Result<CursorPageResponse<SaleListItemResponse>>>;
