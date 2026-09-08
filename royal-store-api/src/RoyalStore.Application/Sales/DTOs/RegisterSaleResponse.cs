namespace RoyalStore.Application.Sales.DTOs;

public record RegisterSaleResponse(
    Guid SaleId,
    SyncStatus Status,
    decimal CashbackApplied = 0m,
    decimal CashbackEarned = 0m,
    /// <summary>How much the selected payment method's discount reduced this sale's total (037).
    /// Zero when settled by more than one payment method or when none carried a PaymentId.</summary>
    decimal PaymentDiscountApplied = 0m);
