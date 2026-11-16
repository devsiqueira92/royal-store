using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface IPriceListRepository
{
    Task AddAsync(PriceList priceList, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<PriceList> priceLists, CancellationToken ct = default);
    Task<PriceList?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The store's default list, or null for a store that somehow has none.</summary>
    Task<PriceList?> GetDefaultAsync(CancellationToken ct = default);

    /// <summary>The store's lists, default first, then by name.</summary>
    Task<IReadOnlyList<PriceList>> GetAllAsync(bool? isActive = null, CancellationToken ct = default);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid? excludeId, CancellationToken ct = default);

    /// <summary>True when at least one product is priced on the list (061 FR-006).</summary>
    Task<bool> HasProductPricesAsync(Guid id, CancellationToken ct = default);

    /// <summary>How many products (services excluded) have no price on the list (061 FR-005).</summary>
    Task<int> CountProductsWithoutPriceAsync(Guid id, CancellationToken ct = default);

    /// <summary>Number of products priced on each list, keyed by list id. Lists with none are absent.</summary>
    Task<IReadOnlyDictionary<Guid, int>> GetProductCountsAsync(CancellationToken ct = default);

    /// <summary>
    /// Every product (services excluded) of the store, tracked, with only its price on the given
    /// list loaded — what a default switch needs to re-mirror the legacy price (061 FR-020).
    /// </summary>
    Task<IReadOnlyList<Product>> GetProductsWithPriceOnAsync(Guid id, CancellationToken ct = default);

    /// <summary>How many lists use this one as the base of their percentage rule (062 FR-009).</summary>
    Task<int> CountListsBasedOnAsync(Guid id, CancellationToken ct = default);

    void Remove(PriceList priceList);
}
