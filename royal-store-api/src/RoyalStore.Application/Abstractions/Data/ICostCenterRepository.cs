using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Abstractions.Data;

public interface ICostCenterRepository
{
    Task AddAsync(CostCenter costCenter, CancellationToken ct = default);
    Task<CostCenter?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CostCenter>> GetAllAsync(bool? isActive = null, CancellationToken ct = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId, CancellationToken ct = default);

    /// <summary>True when an expense or a template still points at this cost center (FR-027).</summary>
    Task<bool> IsReferencedAsync(Guid id, CancellationToken ct = default);

    Task UpdateAsync(CostCenter costCenter, CancellationToken ct = default);
    Task RemoveAsync(CostCenter costCenter, CancellationToken ct = default);
}
