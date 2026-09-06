using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class UnitMeasureOptionRepository : IUnitMeasureOptionRepository
{
    private readonly RoyalStoreContext _context;

    public UnitMeasureOptionRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UnitMeasureOption option, CancellationToken ct = default) =>
        await _context.UnitMeasureOptions.AddAsync(option, ct);

    public async Task<UnitMeasureOption?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.UnitMeasureOptions.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<UnitMeasureOption>> GetAllAsync(CancellationToken ct = default) =>
        await _context.UnitMeasureOptions.ToListAsync(ct);

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId, CancellationToken ct = default) =>
        await _context.UnitMeasureOptions.AnyAsync(
            x => x.Code == code && (excludeId == null || x.Id != excludeId),
            ct);

    public Task UpdateAsync(UnitMeasureOption option, CancellationToken ct = default)
    {
        _context.UnitMeasureOptions.Update(option);
        return Task.CompletedTask;
    }
}
