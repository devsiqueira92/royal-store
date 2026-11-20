using RoyalStore.Application.Abstractions.Data;

namespace RoyalStore.Application.Labels;

/// <summary>
/// The first half of a default swap (065 research D7). "One default per store" is a unique index,
/// checked statement by statement, so the current default is unmarked and saved before another
/// label is marked — the 061 price-list pattern. Must run inside the caller's transaction.
/// </summary>
public static class LabelDefaults
{
    /// <returns>The id of the label that stopped being the default, or null when none did.</returns>
    public static async Task<Guid?> ReleaseAsync(
        ILabelRepository repository, IUnitOfWork unitOfWork, Guid? keepId, CancellationToken ct)
    {
        var current = await repository.GetDefaultAsync(ct);
        if (current is null || current.Id == keepId)
            return null;

        current.UnmarkDefault();
        await unitOfWork.SaveChangesAsync(ct);
        return current.Id;
    }
}
