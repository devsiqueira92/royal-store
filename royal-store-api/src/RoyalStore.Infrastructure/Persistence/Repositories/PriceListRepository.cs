using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class PriceListRepository : IPriceListRepository
{
    private readonly RoyalStoreContext _context;

    public PriceListRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PriceList priceList, CancellationToken ct = default) =>
        await _context.PriceLists.AddAsync(priceList, ct);

    public async Task AddRangeAsync(IEnumerable<PriceList> priceLists, CancellationToken ct = default) =>
        await _context.PriceLists.AddRangeAsync(priceLists, ct);

    public async Task<PriceList?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.PriceLists.FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<PriceList?> GetDefaultAsync(CancellationToken ct = default) =>
        await _context.PriceLists.FirstOrDefaultAsync(l => l.IsDefault, ct);

    public async Task<IReadOnlyList<PriceList>> GetAllAsync(bool? isActive = null, CancellationToken ct = default)
    {
        var query = _context.PriceLists.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(l => l.IsActive == isActive.Value);

        return await query
            .OrderByDescending(l => l.IsDefault)
            .ThenBy(l => l.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNormalizedNameAsync(
        string normalizedName, Guid? excludeId, CancellationToken ct = default) =>
        await _context.PriceLists.AnyAsync(
            l => l.NormalizedName == normalizedName && (excludeId == null || l.Id != excludeId), ct);

    public async Task<bool> HasProductPricesAsync(Guid id, CancellationToken ct = default) =>
        await _context.ProductPrices.AnyAsync(pp => pp.PriceListId == id, ct);

    public async Task<int> CountProductsWithoutPriceAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products.CountAsync(
            p => p.Type == ItemType.Product && !p.Prices.Any(pp => pp.PriceListId == id), ct);

    public async Task<IReadOnlyDictionary<Guid, int>> GetProductCountsAsync(CancellationToken ct = default)
    {
        var counts = await _context.ProductPrices
            .GroupBy(pp => pp.PriceListId)
            .Select(g => new { PriceListId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(c => c.PriceListId, c => c.Count);
    }

    public async Task<IReadOnlyList<Product>> GetProductsWithPriceOnAsync(Guid id, CancellationToken ct = default) =>
        await _context.Products
            .Include(p => p.Prices.Where(pp => pp.PriceListId == id))
            .Where(p => p.Type == ItemType.Product)
            .ToListAsync(ct);

    public async Task<int> CountListsBasedOnAsync(Guid id, CancellationToken ct = default) =>
        await _context.PriceLists.CountAsync(l => l.BasePriceListId == id, ct);

    public void Remove(PriceList priceList) => _context.PriceLists.Remove(priceList);
}
