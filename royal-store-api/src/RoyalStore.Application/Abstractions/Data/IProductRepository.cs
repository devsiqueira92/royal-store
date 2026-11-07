using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken ct = default);
    Task AddVariantAsync(ProductVariant variant, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(
        bool? isActive, Guid? categoryId, StockStatus? stockStatus, Guid? supplierId,
        string? normalizedSearchTerm, CancellationToken ct = default);
    Task<ProductsSummary> GetSummaryAsync(Guid? categoryId, Guid? supplierId, CancellationToken ct = default);
    Task UpdateAsync(Product product, int? expectedVersion, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetForCatalogAsync(DateTimeOffset? since, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetForPosAsync(Guid? categoryId, CancellationToken ct = default);

    /// <summary>
    /// Loads, in a single round trip, the active products owning the supplied variants together with
    /// those variants and each product's tax profile template — everything a checkout needs to
    /// enrich its lines. Returns tracked entities, so the caller's later writes to the same
    /// instances (e.g. debiting stock) are picked up by SaveChanges. Tenant isolation comes from the
    /// global query filters.
    /// </summary>
    Task<IReadOnlyList<Product>> GetForCheckoutAsync(
        IReadOnlyCollection<Guid> variantIds, CancellationToken ct = default);
}
