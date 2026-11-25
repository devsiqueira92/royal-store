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

    /// <summary>Up to <paramref name="take"/> sales of the current tenant matching <paramref name="filter"/>,
    /// newest first (SoldAt, then Id, both descending), strictly after <paramref name="after"/> in that
    /// order (<c>null</c> → from the newest), with the seller's display name already resolved (067).</summary>
    Task<IReadOnlyList<SaleWithNamesProjection>> GetPageAsync(
        SalesListFilter filter,
        SalesListCursor? after,
        int take,
        CancellationToken ct = default);
}
