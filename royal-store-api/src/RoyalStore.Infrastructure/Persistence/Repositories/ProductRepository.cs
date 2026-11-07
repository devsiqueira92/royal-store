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
        bool? isActive, Guid? categoryId, StockStatus? stockStatus, Guid? supplierId,
        string? normalizedSearchTerm, CancellationToken ct = default)
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

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        // Mirrors Product.GetStockStatus() as an EF-translatable predicate: EF Core cannot
        // translate a call to that method into SQL. Keep both in sync.
        if (stockStatus.HasValue)
            query = stockStatus.Value switch
            {
                StockStatus.OutOfStock => query.Where(p =>
                    p.StockTrack && p.Variants.Any(v => v.StockQuantity != null && v.StockQuantity <= 0)),
                StockStatus.LowStock => query.Where(p =>
                    p.StockTrack
                    && !p.Variants.Any(v => v.StockQuantity != null && v.StockQuantity <= 0)
                    && p.Variants.Any(v => v.StockQuantity != null && v.MinStockQuantity != null
                        && v.StockQuantity <= v.MinStockQuantity)),
                StockStatus.InStock => query.Where(p =>
                    !p.StockTrack || !p.Variants.Any(v =>
                        v.StockQuantity != null && (v.StockQuantity <= 0
                            || (v.MinStockQuantity != null && v.StockQuantity <= v.MinStockQuantity)))),
                _ => query,
            };

        if (!string.IsNullOrEmpty(normalizedSearchTerm))
        {
            var pattern = "%" + normalizedSearchTerm + "%";
            query = query.Where(p =>
                EF.Functions.Like(EF.Property<string>(p, "NormalizedName"), pattern) ||
                EF.Functions.Like(EF.Property<string>(p, "NormalizedSku"), pattern) ||
                p.Variants.Any(v => EF.Functions.Like(EF.Property<string>(v, "NormalizedSku"), pattern)));
        }

        return await query.ToListAsync(ct);
    }

    public async Task<ProductsSummary> GetSummaryAsync(
        Guid? categoryId, Guid? supplierId, CancellationToken ct = default)
    {
        var query = _context.Products.AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        // Every product has at least one variant, so a plain (inner) join is enough — no product
        // is ever dropped by it — and GroupBy(x => 1) gets all three aggregates, including a
        // distinct product count, in a single round trip instead of three.
        var totals = await query
            .SelectMany(p => p.Variants, (p, v) => new { p, v })
            .GroupBy(x => 1)
            .Select(g => new
            {
                StockValue = g.Sum(x => (x.v.Price ?? x.p.BasePrice) * (x.v.StockQuantity ?? 0)),
                TotalQuantity = g.Sum(x => x.v.StockQuantity ?? 0),
                Result = g.Select(x => x.p.Id).Distinct().Count()
            })
            .FirstOrDefaultAsync(ct);

        return totals is null
            ? new ProductsSummary(0m, 0m, 0)
            : new ProductsSummary(totals.StockValue, totals.TotalQuantity, totals.Result);
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

        // Tracked, not AsNoTracking: RegisterSaleCommandHandler debits stock on these same
        // instances right after enriching the sale lines, and needs EF to pick up that change.
        return await _context.Products
            .Include(p => p.Variants.Where(v => variantIds.Contains(v.Id)))
            .Include(p => p.TaxProfileTemplate)
            .Where(p => p.IsActive && p.Variants.Any(v => variantIds.Contains(v.Id)))
            .ToListAsync(ct);
    }
}
