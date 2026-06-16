namespace RoyalStore.Application.Sales.DTOs;

public record SyncSalesResponse(IReadOnlyList<SaleSyncItemResult> Results);
