using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Abstractions.Data;

public interface IFinancialAccountRepository
{
    Task AddAsync(FinancialAccount account, CancellationToken ct = default);
    Task<FinancialAccount?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FinancialAccount>> GetAllAsync(bool? isActive = null, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default);

    /// <summary>True when an expense, a payment or a template still points at this account (FR-027).</summary>
    Task<bool> IsReferencedAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Outflow attributed to each account over a period, computed from unreversed payments. This
    /// replaces the stored balance that FR-026a forbids — an account carries no balance column.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetOutflowByAccountAsync(
        DateOnly from, DateOnly to, CancellationToken ct = default);

    Task UpdateAsync(FinancialAccount account, CancellationToken ct = default);
    Task RemoveAsync(FinancialAccount account, CancellationToken ct = default);
}
