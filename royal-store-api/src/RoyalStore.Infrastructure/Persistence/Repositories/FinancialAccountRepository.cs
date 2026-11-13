using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class FinancialAccountRepository : IFinancialAccountRepository
{
    private readonly RoyalStoreContext _context;

    public FinancialAccountRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(FinancialAccount account, CancellationToken ct = default) =>
        await _context.FinancialAccounts.AddAsync(account, ct);

    public async Task<FinancialAccount?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.FinancialAccounts.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<FinancialAccount>> GetAllAsync(
        bool? isActive = null, CancellationToken ct = default)
    {
        var query = _context.FinancialAccounts.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        return await query.OrderBy(a => a.Name).ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default) =>
        await _context.FinancialAccounts.AnyAsync(
            a => a.Name == name && (excludeId == null || a.Id != excludeId), ct);

    public async Task<bool> IsReferencedAsync(Guid id, CancellationToken ct = default) =>
        await _context.Expenses.AnyAsync(e => e.FinancialAccountId == id, ct) ||
        await _context.ExpensePayments.AnyAsync(p => p.FinancialAccountId == id, ct) ||
        await _context.ExpenseRecurrences.AnyAsync(r => r.FinancialAccountId == id, ct);

    /// <summary>
    /// Computed from payments on demand — no balance is stored anywhere (FR-026a). Rows are read
    /// flat and grouped in memory, per the translatability rule (research Decision 8).
    /// </summary>
    public async Task<IReadOnlyDictionary<Guid, decimal>> GetOutflowByAccountAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var rows = await _context.ExpensePayments
            .Where(p => !p.IsReversed && p.PaidAt >= from && p.PaidAt <= to)
            .Select(p => new { p.FinancialAccountId, p.Amount })
            .ToListAsync(ct);

        return rows
            .GroupBy(r => r.FinancialAccountId)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));
    }

    public Task UpdateAsync(FinancialAccount account, CancellationToken ct = default)
    {
        _context.FinancialAccounts.Update(account);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(FinancialAccount account, CancellationToken ct = default)
    {
        _context.FinancialAccounts.Remove(account);
        return Task.CompletedTask;
    }
}
