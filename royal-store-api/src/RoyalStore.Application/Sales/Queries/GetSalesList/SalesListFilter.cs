using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Queries.GetSalesList;

/// <summary>Sales list filters; all optional, combined with AND. Text filters are already normalized.</summary>
public sealed record SalesListFilter(
    string? NormalizedSellerName = null,
    string? NormalizedCustomerName = null,
    DateOnly? SoldFrom = null,
    DateOnly? SoldTo = null,
    FulfillmentMode? FulfillmentMode = null);
