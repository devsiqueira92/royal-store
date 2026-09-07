using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class LoyaltyTierRepository : ILoyaltyTierRepository
{
    private readonly RoyalStoreContext _context;

    public LoyaltyTierRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LoyaltyTier tier, CancellationToken ct = default) =>
        await _context.LoyaltyTiers.AddAsync(tier, ct);

    public async Task AddRangeAsync(IEnumerable<LoyaltyTier> tiers, CancellationToken ct = default) =>
        await _context.LoyaltyTiers.AddRangeAsync(tiers, ct);

    public async Task<LoyaltyTier?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.LoyaltyTiers.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<LoyaltyTier>> GetAllAsync(CancellationToken ct = default) =>
        await _context.LoyaltyTiers.ToListAsync(ct);

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken ct = default) =>
        await _context.LoyaltyTiers.AnyAsync(
            t => t.Name == name && (excludeId == null || t.Id != excludeId), ct);

    public Task UpdateAsync(LoyaltyTier tier, CancellationToken ct = default)
    {
        _context.LoyaltyTiers.Update(tier);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(LoyaltyTier tier, CancellationToken ct = default)
    {
        _context.LoyaltyTiers.Remove(tier);
        return Task.CompletedTask;
    }

    public async Task<LoyaltyTier?> GetStandardTierAsync(Guid tenantId, CancellationToken ct = default) =>
        await _context.LoyaltyTiers.FirstOrDefaultAsync(
            t => t.TenantId == tenantId && t.Name == "Standard", ct);
}
