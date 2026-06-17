using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(bool? isActive, Guid? categoryId, CancellationToken ct = default);
    Task<bool> ExistsBySkuAsync(string sku, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetForCatalogAsync(DateTimeOffset? since, CancellationToken ct = default);
}
