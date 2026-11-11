using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record SaleDetailsResponse(
    Guid Id,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal LoyaltyDiscountApplied,
    decimal PaymentDiscountApplied,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal CashbackApplied,
    string FulfillmentMode,
    string? SellerName,
    string? CustomerName,
    string? CustomerLoyaltyTierName,
    DateTime SoldAt,
    IReadOnlyList<SaleItemDetailsResponse> Items)
{
    public static SaleDetailsResponse From(Sale sale, string? sellerName) => new(
        sale.Id,
        sale.Subtotal,
        sale.DiscountAmount,
        sale.LoyaltyDiscountApplied,
        sale.PaymentDiscountApplied,
        sale.TaxAmount,
        sale.TotalAmount,
        sale.CashbackApplied,
        sale.FulfillmentMode.ToString(),
        sellerName,
        sale.CustomerSnapshot?.Name,
        sale.CustomerSnapshot?.LoyaltyTierName,
        sale.SoldAt,
        sale.Items.Select(SaleItemDetailsResponse.From).ToList());
}
