namespace RoyalStore.Application.Abstractions.Catalog;

public interface ITaxProfileTemplateUsageChecker
{
    Task<bool> IsInUseAsync(Guid templateId, CancellationToken ct = default);
}
