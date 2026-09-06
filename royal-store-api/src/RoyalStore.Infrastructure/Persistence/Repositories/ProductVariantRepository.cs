using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class ProductVariantRepository : IProductVariantRepository
{
    private readonly RoyalStoreContext _context;

    public ProductVariantRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ProductVariant variant, CancellationToken ct = default) =>
        await _context.ProductVariants.AddAsync(variant, ct);


    public async Task RemoveAsync(ProductVariant variant, CancellationToken ct = default) =>
        _context.ProductVariants.Remove(variant);

    public async Task<ProductVariant?> GetById(Guid variant, CancellationToken ct = default) =>
        await _context.ProductVariants.FindAsync(variant, ct);
}
