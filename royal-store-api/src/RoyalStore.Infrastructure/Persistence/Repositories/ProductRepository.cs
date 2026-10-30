using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly RoyalStoreContext _context;

    public ProductRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Product product, CancellationToken ct = default) =>
        await _context.Products.AddAsync(product, ct);

    public async Task AddVariantAsync(ProductVariant variant, CancellationToken ct = default) =>
        await _context.ProductVariants.AddAsync(variant, ct);

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Variants)
            .Include(p => p.Photos)
            .Include(p => p.TaxProfileTemplate)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        bool? isActive, Guid? categoryId, CancellationToken ct = default)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Variants)
            .Include(p => p.Photos)
            .Include(p => p.TaxProfileTemplate)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return await query.ToListAsync(ct);
    }

    public Task UpdateAsync(Product product, int? expectedVersion, CancellationToken ct = default)
    {
        // Every caller loads the product through GetByIdAsync first, so it's already
        // tracked and its mutations are already picked up by SaveChanges. Calling
        // Update() here would re-walk the whole graph and mark it all Modified,
        // including newly added child entities whose client-generated Guid key makes
        // EF mistake them for existing rows (producing a spurious 0-rows-affected
        // concurrency failure instead of an insert). Only attach explicitly if the
        // entity truly isn't tracked yet.
        if (_context.Entry(product).State == EntityState.Detached)
            _context.Products.Update(product);

        // Override the auto-captured OriginalValue (the value we just loaded, which is
        // always "current") with the version the caller last saw, so SaveChanges can
        // detect that the row changed in between, even though this same request only
        // ever sees the up-to-date row.
        if (expectedVersion.HasValue)
            _context.Entry(product).Property(p => p.Version).OriginalValue = expectedVersion.Value;

        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Product>> GetForCatalogAsync(DateTimeOffset? since, CancellationToken ct = default)
    {
        var query = _context.Products.AsQueryable();
        query = since.HasValue
            ? query.Where(p => p.UpdatedAt > since.Value)
            : query.Where(p => p.IsActive);
        return await query.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetForPosAsync(Guid? categoryId, CancellationToken ct = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Variants)
            .Include(p => p.Photos)
            .Include(p => p.TaxProfileTemplate)
            .Where(p => p.IsActive);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetForCheckoutAsync(
        IReadOnlyCollection<Guid> variantIds, CancellationToken ct = default)
    {
        if (variantIds.Count == 0)
            return [];

        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Variants.Where(v => variantIds.Contains(v.Id)))
            .Include(p => p.TaxProfileTemplate)
            .Where(p => p.IsActive && p.Variants.Any(v => variantIds.Contains(v.Id)))
            .ToListAsync(ct);
    }
}
