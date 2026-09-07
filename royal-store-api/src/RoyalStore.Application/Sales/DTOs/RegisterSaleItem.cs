namespace RoyalStore.Application.Sales.DTOs;

/// <summary>
/// One scanned line. The terminal states only what was sold and how many; price, naming and fiscal
/// classification are resolved server-side.
/// </summary>
public record RegisterSaleItem(
    Guid VariantId,
    Guid ProductId,
    decimal Quantity,
    decimal DiscountItem = 0m);
