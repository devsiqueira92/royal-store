using RoyalStore.Domain.Fiscal;

namespace RoyalStore.Application.Abstractions.Data;

public interface IIbptTaxRepository
{
    Task<IbptTax?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<IbptTax>> SearchAsync(string normalizedTerm, int take, CancellationToken ct = default);

    /// <summary>
    /// Returns at most one active IBPT entry per supplied code: the row whose
    /// [EffectiveStart, EffectiveEnd] window contains <paramref name="asOf"/>, or —
    /// when none does — the row with the most recent EffectiveStart.
    /// </summary>
    Task<IReadOnlyDictionary<string, IbptTax>> GetCurrentByCodesAsync(
        IReadOnlyCollection<string> codes, DateOnly asOf, CancellationToken ct = default);
}
