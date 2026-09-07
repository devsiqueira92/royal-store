using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Loyalty;

public sealed class LoyaltyTierUsageChecker : ILoyaltyTierUsageChecker
{
    private readonly RoyalStoreContext _context;

    public LoyaltyTierUsageChecker(RoyalStoreContext context)
    {
        _context = context;
    }

    public Task<bool> IsInUseAsync(Guid tierId, CancellationToken ct = default) =>
        _context.Customers.AnyAsync(c => c.LoyaltyTierId == tierId, ct);
}
