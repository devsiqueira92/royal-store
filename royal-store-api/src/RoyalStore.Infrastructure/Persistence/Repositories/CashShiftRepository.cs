using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Sales;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class CashShiftRepository : ICashShiftRepository
{
    private readonly RoyalStoreContext _context;

    public CashShiftRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CashShift cashShift, CancellationToken ct = default) =>
        await _context.CashShifts.AddAsync(cashShift, ct);

    public async Task<CashShift?> GetOpenByUserIdAsync(string userId, CancellationToken ct = default) =>
        await _context.CashShifts
            .Include(s => s.Movements)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ClosedAt == null, ct);

    public async Task<CashShift?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.CashShifts
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task UpdateAsync(CashShift cashShift, CancellationToken ct = default)
    {
        _context.CashShifts.Update(cashShift);
        return Task.CompletedTask;
    }

    public async Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        new HashSet<Guid>(await _context.CashShifts
            .Where(cs => ids.Contains(cs.Id))
            .Select(cs => cs.Id)
            .ToListAsync(ct));
}
