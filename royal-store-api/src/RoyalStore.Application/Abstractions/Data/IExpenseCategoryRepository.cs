using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Abstractions.Data;

public interface IExpenseCategoryRepository
{
    Task AddAsync(ExpenseCategory category, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<ExpenseCategory> categories, CancellationToken ct = default);
    Task<ExpenseCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// The tenant's categories as a flat list. The hierarchy is built in memory by the caller — a
    /// store has tens of categories, and a recursive CTE cannot be expressed in LINQ
    /// (research Decision 5).
    /// </summary>
    Task<IReadOnlyList<ExpenseCategory>> GetAllAsync(bool? isActive = null, CancellationToken ct = default);

    Task<bool> ExistsByNameAsync(string name, Guid? parentId, Guid? excludeId, CancellationToken ct = default);

    /// <summary>True when an expense, a template or a child category still points at this one (FR-027).</summary>
    Task<bool> IsReferencedAsync(Guid id, CancellationToken ct = default);

    Task UpdateAsync(ExpenseCategory category, CancellationToken ct = default);
    Task RemoveAsync(ExpenseCategory category, CancellationToken ct = default);
}
