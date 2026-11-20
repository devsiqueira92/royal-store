using RoyalStore.Domain.Labels;

namespace RoyalStore.Application.Abstractions.Data;

/// <summary>The store's labels (065). Every read loads the label's format with it.</summary>
public interface ILabelRepository
{
    /// <summary>Adds the label and its format.</summary>
    Task AddAsync(LabelTemplate label, CancellationToken ct = default);

    /// <summary>Tracked, with its format; null when missing or another store's.</summary>
    Task<LabelTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>The store's default label, tracked, or null when the store has none.</summary>
    Task<LabelTemplate?> GetDefaultAsync(CancellationToken ct = default);

    /// <summary>The store's labels with their formats, untracked; default first, then by name.</summary>
    Task<IReadOnlyList<LabelTemplate>> GetAllAsync(bool? isActive = null, CancellationToken ct = default);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid? excludeId, CancellationToken ct = default);

    /// <summary>Removes the label and its format.</summary>
    void Remove(LabelTemplate label);
}
