using RoyalStore.Application.Sales.Queries.GetSalesList;

namespace RoyalStore.Application.Sales.DTOs;

public record SaleListItemResponse(
    Guid Id,
    string? SellerName,
    decimal Subtotal,
    decimal DiscountAmount,
    string? CustomerName,
    DateTime SoldAt,
    string FulfillmentMode)
{
    public static SaleListItemResponse From(SaleWithNamesProjection p) =>
        new(p.Id, p.SellerName, p.Subtotal, p.DiscountAmount, p.CustomerName, p.SoldAt, p.FulfillmentMode.ToString());
}
