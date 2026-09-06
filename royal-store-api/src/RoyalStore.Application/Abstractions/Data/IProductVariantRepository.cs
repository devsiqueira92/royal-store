using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface IProductVariantRepository
{
    Task AddAsync(ProductVariant variant, CancellationToken ct = default);
    Task RemoveAsync(ProductVariant variant, CancellationToken ct = default);
    Task<ProductVariant?> GetById(Guid variant, CancellationToken ct = default);
}
