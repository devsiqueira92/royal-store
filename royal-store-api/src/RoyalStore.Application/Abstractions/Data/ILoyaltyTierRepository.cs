using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Abstractions.Data;

public interface ILoyaltyTierRepository
{
    Task AddAsync(LoyaltyTier tier, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<LoyaltyTier> tiers, CancellationToken ct = default);
    Task<LoyaltyTier?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LoyaltyTier>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(LoyaltyTier tier, CancellationToken ct = default);
    Task DeleteAsync(LoyaltyTier tier, CancellationToken ct = default);

    /// <summary>Resolves the tenant's Standard tier — the default a customer is assigned when no tier
    /// is explicitly chosen (FR-013). Every tenant is guaranteed to have one via seeding.</summary>
    Task<LoyaltyTier?> GetStandardTierAsync(Guid tenantId, CancellationToken ct = default);
}
