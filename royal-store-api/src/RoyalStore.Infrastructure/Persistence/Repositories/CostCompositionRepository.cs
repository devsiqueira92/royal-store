using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class CostCompositionRepository : ICostCompositionRepository
{
    private readonly RoyalStoreContext _context;

    public CostCompositionRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CostCompositionList list, CancellationToken ct = default) =>
        await _context.CostCompositionLists.AddAsync(list, ct);

    public async Task<CostCompositionList?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.CostCompositionLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<CostCompositionSummary>> GetSummariesAsync(CancellationToken ct = default) =>
        await _context.CostCompositionLists
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .ThenBy(l => l.CreatedAt)
            .Select(l => new CostCompositionSummary(l.Id, l.ProductId, l.Name, l.TotalValue, l.Items.Count))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CostCompositionList>> GetUnlinkedAsync(CancellationToken ct = default) =>
        await _context.CostCompositionLists
            .AsNoTracking()
            .Include(l => l.Items)
            .Where(l => l.ProductId == null)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

    public void Remove(CostCompositionList list) => _context.CostCompositionLists.Remove(list);

    public async Task AddItemAsync(ProductCostItem item, CancellationToken ct = default) =>
        await _context.ProductCostItems.AddAsync(item, ct);

    public void RemoveItem(ProductCostItem item) => _context.ProductCostItems.Remove(item);
}
