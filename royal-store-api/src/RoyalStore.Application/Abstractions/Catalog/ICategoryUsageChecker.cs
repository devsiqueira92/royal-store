namespace RoyalStore.Application.Abstractions.Catalog;

public interface ICategoryUsageChecker
{
    Task<bool> IsInUseAsync(Guid categoryId, CancellationToken ct = default);
}
