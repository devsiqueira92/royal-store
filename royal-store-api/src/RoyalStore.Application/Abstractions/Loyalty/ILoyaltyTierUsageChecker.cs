namespace RoyalStore.Application.Abstractions.Loyalty;

/// <summary>Mirrors <c>ICategoryUsageChecker</c> — guards tier deletion against FR-014.</summary>
public interface ILoyaltyTierUsageChecker
{
    Task<bool> IsInUseAsync(Guid tierId, CancellationToken ct = default);
}
