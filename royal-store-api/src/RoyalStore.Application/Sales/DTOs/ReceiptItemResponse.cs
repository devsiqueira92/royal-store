namespace RoyalStore.Application.Sales.DTOs;

public record ReceiptItemResponse(
    string ProductName, decimal Quantity, decimal UnitPrice, decimal TotalPrice, string? Sku,
    decimal TaxPercentage, decimal TaxAmount);
