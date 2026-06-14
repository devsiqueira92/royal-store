using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Catalog;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Catalog;

public sealed class CategoryUsageChecker : ICategoryUsageChecker
{
    private readonly RoyalStoreContext _context;

    public CategoryUsageChecker(RoyalStoreContext context)
    {
        _context = context;
    }

    public Task<bool> IsInUseAsync(Guid categoryId, CancellationToken ct = default) =>
        _context.Products.AnyAsync(p => p.CategoryId == categoryId && p.IsActive, ct);
}
