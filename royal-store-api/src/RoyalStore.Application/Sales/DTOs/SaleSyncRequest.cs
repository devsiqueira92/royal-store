namespace RoyalStore.Application.Sales.DTOs;

public record SaleSyncRequest(
    Guid Id,
    Guid CashShiftId,
    decimal TotalAmount,
    decimal DiscountAmount,
    DateTime SoldAt,
    IReadOnlyList<SaleItemRequest> Items,
    IReadOnlyList<SalePaymentRequest> Payments);
