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

    public async Task<CashShift?> GetByIdWithMovementsAsync(Guid id, CancellationToken ct = default) =>
        await _context.CashShifts
            .AsNoTracking()
            .Include(s => s.Movements)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    // One statement: operator name and movement totals are correlated subqueries, so a user with
    // several Seller rows can't duplicate a shift. The UserId != null guard stops EF's C# null
    // semantics from matching an operator-less shift to a Seller without a user. Supplements/withdrawals
    // use the same In/Out predicates as CashShift.SumSupplements/SumWithdrawals; Sale entries count
    // only as movements.
    public async Task<IReadOnlyList<CashShiftWithMovementTotals>> GetAllWithMovementTotalsAsync(CancellationToken ct = default) =>
        await _context.CashShifts
            .AsNoTracking()
            .OrderByDescending(s => s.OpenedAt)
            .ThenBy(s => s.Id)
            .Select(s => new CashShiftWithMovementTotals(
                s.Id,
                s.UserId,
                _context.Sellers.Where(se => se.UserId != null && se.UserId == s.UserId).Select(se => se.FullName).FirstOrDefault()
                    ?? _context.Users.Where(u => u.Id == s.UserId).Select(u => u.UserName).FirstOrDefault(),
                s.StartingCash,
                s.OpenedAt,
                s.ClosedAt,
                s.Movements.Where(m => m.Type == MovementType.In).Sum(m => m.Amount),
                s.Movements.Where(m => m.Type == MovementType.Out).Sum(m => m.Amount),
                s.Movements.Count))
            .ToListAsync(ct);

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

    // Explicit Add: a movement with a preset Guid key discovered only through CashShift.Movements
    // would be tracked as Modified by DetectChanges and issue an UPDATE for a row that doesn't exist.
    public async Task AddMovementAsync(CashMovement movement, CancellationToken ct = default) =>
        await _context.CashMovements.AddAsync(movement, ct);
}
