using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface ICategoryRepository
{
    Task AddAsync(Category category, CancellationToken ct = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryWithProductCount>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(Category category, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetForCatalogAsync(DateTimeOffset? since, CancellationToken ct = default);
    Task<bool> HasProductsAsync(Guid categoryId, CancellationToken ct = default);
    Task DeleteAsync(Category category, CancellationToken ct = default);
}
