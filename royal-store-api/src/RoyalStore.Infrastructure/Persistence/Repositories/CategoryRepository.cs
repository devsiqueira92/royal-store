using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly RoyalStoreContext _context;

    public CategoryRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Category category, CancellationToken ct = default) =>
        await _context.Categories.AddAsync(category, ct);

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CategoryWithProductCount>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Categories
            .Select(c => new CategoryWithProductCount(c, c.Products.Count))
            .ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default) =>
        await _context.Categories.AnyAsync(
            c => c.Name == name && (excludeId == null || c.Id != excludeId),
            ct);

    public Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        _context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Category>> GetForCatalogAsync(DateTimeOffset? since, CancellationToken ct = default)
    {
        var query = _context.Categories.AsQueryable();
        query = since.HasValue
            ? query.Where(c => c.UpdatedAt > since.Value)
            : query.Where(c => c.IsActive);
        return await query.ToListAsync(ct);
    }
}
