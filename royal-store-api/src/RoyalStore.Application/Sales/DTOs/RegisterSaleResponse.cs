namespace RoyalStore.Application.Sales.DTOs;

public record RegisterSaleResponse(
    Guid SaleId,
    SyncStatus Status,
    decimal CashbackApplied = 0m,
    decimal CashbackEarned = 0m);
