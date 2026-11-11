using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Application.Sales.Queries.GetSalesList;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Abstractions.Data;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken ct = default);
    Task AddItemsAsync(IEnumerable<SaleItem> items, CancellationToken ct = default);
    Task AddPaymentsAsync(IEnumerable<SalePayment> payments, CancellationToken ct = default);
    Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The full sale aggregate (with its <see cref="Sale.Items"/> loaded) plus the seller's
    /// display name, already resolved with the same profile-first, login-name-fallback rule used
    /// elsewhere. <c>null</c> when no such sale exists for the current tenant.</summary>
    Task<SaleWithSellerName?> GetDetailsByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The sale, its store name, and its items/payments already resolved into
    /// <see cref="ReceiptItemResponse"/>/<see cref="ReceiptPaymentResponse"/> DTOs (approximate tax
    /// burden computed, payment method names resolved) — everything needed to print the sale's customer
    /// receipt. <c>null</c> when no such sale exists for the current tenant.</summary>
    Task<SaleReceiptData?> GetReceiptByIdAsync(Guid id, CancellationToken ct = default);

    Task UpdateAsync(Sale sale, CancellationToken ct = default);
    Task<ShiftSalesTotals> GetShiftSalesTotalsAsync(Guid cashShiftId, CancellationToken ct = default);

    /// <summary>Sales totals of every shift of the current tenant, keyed by shift id, using the same
    /// classification as <see cref="GetShiftSalesTotalsAsync"/>. Shifts with no sales are absent.</summary>
    Task<IReadOnlyDictionary<Guid, ShiftSalesTotals>> GetSalesTotalsByShiftAsync(CancellationToken ct = default);

    /// <summary>Every sale of the current tenant, newest first, with the seller's display name
    /// already resolved. All filters are optional and combine with AND; text filters are already
    /// normalized by the caller.</summary>
    Task<IReadOnlyList<SaleWithNamesProjection>> GetAllAsync(
        string? normalizedSellerName = null,
        string? normalizedCustomerName = null,
        DateOnly? soldFrom = null,
        DateOnly? soldTo = null,
        FulfillmentMode? fulfillmentMode = null,
        CancellationToken ct = default);
}
