using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class TaxProfileTemplateRepository : ITaxProfileTemplateRepository
{
    private readonly RoyalStoreContext _context;

    public TaxProfileTemplateRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(TaxProfileTemplate template, CancellationToken ct = default) =>
        await _context.TaxProfileTemplates.AddAsync(template, ct);

    public async Task<TaxProfileTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.TaxProfileTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<TaxProfileTemplate>> GetAllAsync(ItemType? targetItemType, CancellationToken ct = default)
    {
        var query = _context.TaxProfileTemplates.AsQueryable();

        if (targetItemType.HasValue)
            query = query.Where(t => t.TargetItemType == targetItemType.Value);

        return await query.OrderBy(t => t.Name).ToListAsync(ct);
    }

    public Task UpdateAsync(TaxProfileTemplate template, CancellationToken ct = default)
    {
        if (_context.Entry(template).State == EntityState.Detached)
            _context.TaxProfileTemplates.Update(template);

        return Task.CompletedTask;
    }
}
