using RoyalStore.Application.Abstractions.Catalog;

namespace RoyalStore.Infrastructure.Catalog;

public sealed class StubCategoryUsageChecker : ICategoryUsageChecker
{
    public Task<bool> IsInUseAsync(Guid categoryId, CancellationToken ct = default) =>
        Task.FromResult(false);
}
