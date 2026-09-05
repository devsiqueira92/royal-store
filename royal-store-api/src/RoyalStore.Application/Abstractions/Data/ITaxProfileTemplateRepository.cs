using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface ITaxProfileTemplateRepository
{
    Task AddAsync(TaxProfileTemplate template, CancellationToken ct = default);
    Task<TaxProfileTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TaxProfileTemplate>> GetAllAsync(ItemType? targetItemType, CancellationToken ct = default);
    Task UpdateAsync(TaxProfileTemplate template, CancellationToken ct = default);
}
