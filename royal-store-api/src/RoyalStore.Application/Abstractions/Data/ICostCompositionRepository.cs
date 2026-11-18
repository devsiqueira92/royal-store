using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Abstractions.Data;

public interface ICostCompositionRepository
{
    Task AddAsync(CostCompositionList list, CancellationToken ct = default);

    /// <summary>The list with its lines, tracked. Tenant isolation comes from the global query
    /// filter: a list of another store comes back as null.</summary>
    Task<CostCompositionList?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Every list of the store, linked or not, by name, with the lines counted by the database
    /// rather than loaded (064 FR-028).</summary>
    Task<IReadOnlyList<CostCompositionSummary>> GetSummariesAsync(CancellationToken ct = default);

    /// <summary>The store's lists linked to no product, newest first, each with its lines (064 FR-025).</summary>
    Task<IReadOnlyList<CostCompositionList>> GetUnlinkedAsync(CancellationToken ct = default);

    void Remove(CostCompositionList list);

    /// <summary>Registers a line added to an already-tracked list (see <see cref="ProductCostItemChanges"/>).</summary>
    Task AddItemAsync(ProductCostItem item, CancellationToken ct = default);

    /// <summary>Registers the removal of a line taken off an already-tracked list.</summary>
    void RemoveItem(ProductCostItem item);
}
