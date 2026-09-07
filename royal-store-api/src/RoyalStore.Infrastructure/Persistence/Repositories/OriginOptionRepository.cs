using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class OriginOptionRepository : IOriginOptionRepository
{
    private readonly RoyalStoreContext _context;

    public OriginOptionRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OriginOption option, CancellationToken ct = default) =>
        await _context.OriginOptions.AddAsync(option, ct);

    public async Task<OriginOption?> GetByCodeAsync(string code, CancellationToken ct = default) =>
        await _context.OriginOptions.FirstOrDefaultAsync(x => x.Code == code, ct);

    public async Task<IReadOnlyList<OriginOption>> GetAllAsync(CancellationToken ct = default) =>
        await _context.OriginOptions.ToListAsync(ct);

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId, CancellationToken ct = default) =>
        await _context.OriginOptions.AnyAsync(
            x => x.Code == code && (excludeId == null || x.Id != excludeId),
            ct);

    public Task UpdateAsync(OriginOption option, CancellationToken ct = default)
    {
        _context.OriginOptions.Update(option);
        return Task.CompletedTask;
    }
}
