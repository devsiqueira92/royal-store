using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class ExpenseCategoryRepository : IExpenseCategoryRepository
{
    private readonly RoyalStoreContext _context;

    public ExpenseCategoryRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(ExpenseCategory category, CancellationToken ct = default) =>
        await _context.ExpenseCategories.AddAsync(category, ct);

    public async Task AddRangeAsync(IEnumerable<ExpenseCategory> categories, CancellationToken ct = default) =>
        await _context.ExpenseCategories.AddRangeAsync(categories, ct);

    public async Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(
        bool? isActive = null, CancellationToken ct = default)
    {
        var query = _context.ExpenseCategories.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNameAsync(
        string name, Guid? parentId, Guid? excludeId, CancellationToken ct = default) =>
        await _context.ExpenseCategories.AnyAsync(
            c => c.Name == name && c.ParentId == parentId && (excludeId == null || c.Id != excludeId),
            ct);

    public async Task<bool> IsReferencedAsync(Guid id, CancellationToken ct = default) =>
        await _context.Expenses.AnyAsync(e => e.ExpenseCategoryId == id, ct) ||
        await _context.ExpenseRecurrences.AnyAsync(r => r.ExpenseCategoryId == id, ct) ||
        await _context.ExpenseCategories.AnyAsync(c => c.ParentId == id, ct);

    public Task UpdateAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        _context.ExpenseCategories.Update(category);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ExpenseCategory category, CancellationToken ct = default)
    {
        _context.ExpenseCategories.Remove(category);
        return Task.CompletedTask;
    }
}
