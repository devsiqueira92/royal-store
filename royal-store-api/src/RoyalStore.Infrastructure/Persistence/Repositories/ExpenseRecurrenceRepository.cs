using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class ExpenseRecurrenceRepository : IExpenseRecurrenceRepository
{
    private readonly RoyalStoreContext _context;

    public ExpenseRecurrenceRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ExpenseRecurrence recurrence, CancellationToken ct = default) =>
        await _context.ExpenseRecurrences.AddAsync(recurrence, ct);

    public async Task<ExpenseRecurrence?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.ExpenseRecurrences.FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<ExpenseRecurrence>> GetAllAsync(CancellationToken ct = default) =>
        await _context.ExpenseRecurrences
            .OrderBy(r => r.Description)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ExpenseRecurrence>> GetActiveAsync(CancellationToken ct = default) =>
        await _context.ExpenseRecurrences
            .Where(r => r.IsActive)
            .OrderBy(r => r.Description)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> GetAllTenantIdsAsync(CancellationToken ct = default) =>
        await _context.Tenants.Select(t => t.Id).ToListAsync(ct);

    public Task UpdateAsync(ExpenseRecurrence recurrence, CancellationToken ct = default)
    {
        _context.ExpenseRecurrences.Update(recurrence);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ExpenseRecurrence recurrence, CancellationToken ct = default)
    {
        _context.ExpenseRecurrences.Remove(recurrence);
        return Task.CompletedTask;
    }
}
