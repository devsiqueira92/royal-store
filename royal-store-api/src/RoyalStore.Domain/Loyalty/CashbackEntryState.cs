namespace RoyalStore.Domain.Loyalty;

/// <summary>
/// The stored lifecycle of a <see cref="CashbackEntry"/>. Deliberately has no "Expired" member —
/// expiry is a computed fact (<c>ExpiresAt &lt; now</c>), never a stored transition. See
/// <see cref="CashbackEntry"/> remarks for the rationale.
/// </summary>
public enum CashbackEntryState
{
    Available,
    Used
}
