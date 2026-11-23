using RoyalStore.Application.DTOs;
using RoyalStore.Application.Labels.DTOs;
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

    /// <summary>One listing row per non-system label, projected in the database; default first, then by name.</summary>
    Task<IReadOnlyList<LabelSummaryResponse>> GetSummariesAsync(bool? isActive = null, CancellationToken ct = default);

    /// <summary>The store's active labels as selection options (code = id, description = name); default first, then by name.</summary>
    Task<IReadOnlyList<OptionResponse>> GetActiveOptionsAsync(CancellationToken ct = default);

    /// <summary>
    /// The system configurations plus the store's active labels as options (code = id, description =
    /// name); system first, then by name (066).
    /// </summary>
    Task<IReadOnlyList<OptionResponse>> GetConfigurationOptionsAsync(CancellationToken ct = default);

    /// <summary>
    /// The labels a print can use as options (code = id, description = name): the store's active labels,
    /// default first, then by name; then the system configurations by name.
    /// </summary>
    Task<IReadOnlyList<OptionResponse>> GetPrintOptionsAsync(CancellationToken ct = default);

    /// <summary>Among the store's own labels only — system configuration names do not count (066 FR-007).</summary>
    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid? excludeId, CancellationToken ct = default);

    /// <summary>Removes the label and its format.</summary>
    void Remove(LabelTemplate label);
}
