using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Loyalty;

/// <summary>
/// The single place effective cashback-entry state is computed (data-model.md §CashbackEntry). A
/// stored <see cref="CashbackEntryState.Used"/> always wins; otherwise an entry past its
/// <see cref="CashbackEntry.ExpiresAt"/> is effectively expired regardless of its stored state
/// (FR-030) — reused by history, balance and redemption-eligibility checks alike so the rule is never
/// reimplemented per call site.
/// </summary>
public static class CashbackEntryStateResolver
{
    public static EffectiveCashbackState Resolve(CashbackEntry entry, DateTime now) =>
        entry.State == CashbackEntryState.Used
            ? EffectiveCashbackState.Used
            : entry.ExpiresAt < now
                ? EffectiveCashbackState.Expired
                : EffectiveCashbackState.Available;

    public static bool IsEffectivelyAvailable(CashbackEntry entry, DateTime now) =>
        Resolve(entry, now) == EffectiveCashbackState.Available;
}

public enum EffectiveCashbackState
{
    Available,
    Used,
    Expired
}
