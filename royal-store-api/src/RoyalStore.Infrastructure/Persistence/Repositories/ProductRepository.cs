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

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Tags).ThenInclude(t => t.Category)
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetAllAsync(
        bool? isActive, Guid? categoryId, CancellationToken ct = default)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Tags).ThenInclude(t => t.Category)
            .Include(p => p.Photos)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        return await query.ToListAsync(ct);
    }

    public Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
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
}
