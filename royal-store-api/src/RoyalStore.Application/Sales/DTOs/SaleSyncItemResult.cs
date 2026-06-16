namespace RoyalStore.Application.Sales.DTOs;

public record SaleSyncItemResult(Guid SaleId, SyncStatus Status, string? Error = null);
