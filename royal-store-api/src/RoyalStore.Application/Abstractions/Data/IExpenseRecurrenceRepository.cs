using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Abstractions.Data;

public interface IExpenseRecurrenceRepository
{
    Task AddAsync(ExpenseRecurrence recurrence, CancellationToken ct = default);
    Task<ExpenseRecurrence?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ExpenseRecurrence>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Active templates for the current tenant — what a generation run iterates.</summary>
    Task<IReadOnlyList<ExpenseRecurrence>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Every tenant id in the system, for the worker to iterate. Tenants carry no global query
    /// filter, so this crosses tenants by design and needs no filter bypass.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetAllTenantIdsAsync(CancellationToken ct = default);

    Task UpdateAsync(ExpenseRecurrence recurrence, CancellationToken ct = default);
    Task RemoveAsync(ExpenseRecurrence recurrence, CancellationToken ct = default);
}
