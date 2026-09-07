using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Loyalty;

/// <summary>
/// One amount earned by one customer from one purchase. Tracks how much of its grant is still
/// spendable via <see cref="RemainingAmount"/> rather than being redeemed atomically whole, so a
/// customer never forfeits an unspent surplus (FR-025a).
/// </summary>
/// <remarks>
/// <see cref="State"/> only ever holds <see cref="CashbackEntryState.Available"/> or
/// <see cref="CashbackEntryState.Used"/> — there is deliberately no stored "Expired" state. FR-030
/// requires an entry to be treated as expired everywhere (balance, redemption, display) "whether or
/// not anything has actively marked it so"; a stored transition could fall out of sync with
/// <see cref="ExpiresAt"/> if nothing ever ran a sweep. Callers must compute effective state via
/// <c>ExpiresAt &lt; now</c> taking priority over a stored <c>Available</c> (see
/// <c>CashbackEntryStateResolver</c> in the Application layer, the single place this is computed).
/// </remarks>
public sealed class CashbackEntry : Entity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid SaleId { get; private set; }
    public decimal GrantedAmount { get; private set; }
    public decimal RemainingAmount { get; private set; }
    public DateTime EarnedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public CashbackEntryState State { get; private set; }

    /// <summary>EF Core optimistic concurrency token — mirrors <c>Product.Version</c>. Guards against
    /// two terminals consuming the same entry at once (research.md Decision 5).</summary>
    public int Version { get; private set; }

    private CashbackEntry() : base(Guid.NewGuid()) { }

    private CashbackEntry(Guid id, Guid tenantId, Guid customerId, Guid saleId,
        decimal grantedAmount, DateTime earnedAt, DateTime expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        SaleId = saleId;
        GrantedAmount = grantedAmount;
        RemainingAmount = grantedAmount;
        EarnedAt = earnedAt;
        ExpiresAt = expiresAt;
        State = CashbackEntryState.Available;
    }

    public static Result<CashbackEntry> Create(Guid tenantId, Guid customerId, Guid saleId,
        decimal grantedAmount, DateTime earnedAt, int validityDays)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("cashbackentry.tenant_required", "Tenant identifier is required.");

        if (customerId == Guid.Empty)
            return Error.Validation("cashbackentry.customer_required", "Customer identifier is required.");

        if (saleId == Guid.Empty)
            return Error.Validation("cashbackentry.sale_required", "Sale identifier is required.");

        if (grantedAmount <= 0)
            return Error.Validation("cashbackentry.amount_invalid", "Granted amount must be greater than zero.");

        if (validityDays <= 0)
            return Error.Validation("cashbackentry.validity_invalid", "Validity period must be greater than zero days.");

        var expiresAt = earnedAt.AddDays(validityDays);

        return new CashbackEntry(Guid.NewGuid(), tenantId, customerId, saleId, grantedAmount, earnedAt, expiresAt);
    }

    /// <summary>
    /// Applies <paramref name="amount"/> against this entry's remaining balance (FR-025). Callers are
    /// responsible for having already established the entry is effectively available (not expired,
    /// not already used) before calling — this method only enforces the entry's own arithmetic
    /// invariants, not the clock (data-model.md: entities stay free of ambient state).
    /// </summary>
    public Result Consume(decimal amount)
    {
        if (State != CashbackEntryState.Available)
            return Result.Failure(Error.Validation("cashbackentry.not_available",
                "Cashback entry is not available for use."));

        if (amount <= 0)
            return Result.Failure(Error.Validation("cashbackentry.consume_amount_invalid",
                "Amount to consume must be greater than zero."));

        if (amount > RemainingAmount)
            return Result.Failure(Error.Validation("cashbackentry.insufficient_remaining",
                "Amount exceeds the entry's remaining balance."));

        RemainingAmount -= amount;
        if (RemainingAmount == 0)
            State = CashbackEntryState.Used;

        Version++;
        return Result.Success();
    }

    /// <summary>
    /// Reversal only (FR-022d): gives back value previously consumed from this entry. Never exceeds
    /// the original grant (FR-025c). The caller (the ledger) decides, by comparing
    /// <paramref name="expiresAt"/> against "now", whether restoring is meaningful at all — if the
    /// entry's own expiry has already passed, the ledger creates a fresh, already-expired allotment
    /// instead of calling this (FR-022e); this method itself does not consult the clock.
    /// </summary>
    public Result Restore(decimal amount, DateTime expiresAt)
    {
        if (amount <= 0)
            return Result.Failure(Error.Validation("cashbackentry.restore_amount_invalid",
                "Amount to restore must be greater than zero."));

        var restored = RemainingAmount + amount;
        if (restored > GrantedAmount)
            return Result.Failure(Error.Validation("cashbackentry.restore_exceeds_grant",
                "Restored amount cannot exceed the amount originally granted."));

        RemainingAmount = restored;
        ExpiresAt = expiresAt;
        State = CashbackEntryState.Available;
        Version++;
        return Result.Success();
    }

    /// <summary>
    /// Reversal only (FR-022a): the purchase that earned this entry was cancelled/refunded, so the
    /// entire grant is undone regardless of how much was already spent. Idempotent — calling it again
    /// on an already-revoked entry is a no-op success, which is what lets the ledger's
    /// <c>(TenantId, SaleId)</c>-unique <c>Clawback</c> record be the single source of truth for
    /// "has this sale already been reversed" (FR-022f), rather than every entry needing its own guard.
    /// </summary>
    public Result Revoke()
    {
        if (RemainingAmount == 0 && State == CashbackEntryState.Used)
            return Result.Success();

        RemainingAmount = 0;
        State = CashbackEntryState.Used;
        Version++;
        return Result.Success();
    }
}
