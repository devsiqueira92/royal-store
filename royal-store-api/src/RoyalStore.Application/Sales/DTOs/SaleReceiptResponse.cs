using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record SaleReceiptResponse(
    string ReceiptCode, string SoldAt, string? CustomerName, string? StoreName,
    decimal Subtotal, decimal DiscountAmount, decimal TaxAmount, decimal TotalAmount,
    IReadOnlyList<ReceiptItemResponse> Items, IReadOnlyList<ReceiptPaymentResponse> Payments)
{
    public static SaleReceiptResponse From(
        Sale sale, string? storeName, decimal discountAmount,
        IReadOnlyList<ReceiptItemResponse> items, IReadOnlyList<ReceiptPaymentResponse> payments)
    {
        var receiptCode = sale.Id.ToString().Split('-')[^1];

        return new(
            receiptCode, sale.SoldAt.ToString("dd/MM/yyyy HH:mm:ss"), sale.CustomerSnapshot?.Name, storeName,
            sale.Subtotal, discountAmount, sale.TaxAmount, sale.TotalAmount,
            items, payments);
    }
}
