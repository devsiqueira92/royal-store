using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class CostCenterRepository : ICostCenterRepository
{
    private readonly RoyalStoreContext _context;

    public CostCenterRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(CostCenter costCenter, CancellationToken ct = default) =>
        await _context.CostCenters.AddAsync(costCenter, ct);

    public async Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.CostCenters.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CostCenter>> GetAllAsync(
        bool? isActive = null, CancellationToken ct = default)
    {
        var query = _context.CostCenters.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        return await query.OrderBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId, CancellationToken ct = default) =>
        await _context.CostCenters.AnyAsync(
            c => c.Code == code && (excludeId == null || c.Id != excludeId), ct);

    public async Task<bool> IsReferencedAsync(Guid id, CancellationToken ct = default) =>
        await _context.Expenses.AnyAsync(e => e.CostCenterId == id, ct) ||
        await _context.ExpenseRecurrences.AnyAsync(r => r.CostCenterId == id, ct);

    public Task UpdateAsync(CostCenter costCenter, CancellationToken ct = default)
    {
        _context.CostCenters.Update(costCenter);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(CostCenter costCenter, CancellationToken ct = default)
    {
        _context.CostCenters.Remove(costCenter);
        return Task.CompletedTask;
    }
}
