namespace RoyalStore.Application.Sales.DTOs;

public record SaleItemRequest(
    Guid ProductId,
    string ProductName,
    string? ProductCode,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Ncm,
    string? Cest);
