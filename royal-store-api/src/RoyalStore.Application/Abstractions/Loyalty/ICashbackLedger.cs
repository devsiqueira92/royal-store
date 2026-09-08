using RoyalStore.Domain.Common;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Abstractions.Loyalty;

/// <summary>
/// Encapsulates the concurrency-safe read-modify-write sequences a plain CRUD repository can't
/// safely expose (research.md Decisions 2 and 5). Consumed from <c>RegisterSaleCommandHandler</c> and
/// <c>ReverseSaleCashbackCommandHandler</c> so redemption/accrual/reversal commit atomically with the
/// sale they belong to, in the same transaction.
/// </summary>
public interface ICashbackLedger
{
    /// <summary>Applies up to <paramref name="requestedAmount"/> of the customer's available,
    /// unexpired cashback to <paramref name="saleId"/>, consuming soonest-expiring entries first
    /// (FR-028a). Returns the amount actually applied — never more than requested, never more than
    /// what's usable (FR-027).</summary>
    Task<Result<decimal>> RedeemAsync(
        Guid tenantId, Guid customerId, decimal requestedAmount, Guid saleId, CancellationToken ct = default);

    /// <summary>Grants cashback for <paramref name="saleId"/> at the customer's tier rate on
    /// <paramref name="earningBase"/> (FR-016). Returns the amount granted — 0 when the tier is 0% or
    /// the base is non-positive (FR-016c, FR-020); no entry is created in that case.</summary>
    Task<Result<decimal>> AccrueAsync(
        Guid tenantId, Guid customerId, Guid saleId, decimal earningBase, DateTime earnedAt, CancellationToken ct = default);

    /// <summary>Reverses both directions of cashback for a purchase (FR-022): claws back what it
    /// granted (even into a negative balance if already spent) and restores what it consumed.
    /// Idempotent per sale — a second call for the same <paramref name="saleId"/> fails with
    /// <c>sale.already_reversed</c> (FR-022f).</summary>
    Task<Result<CashbackReversalOutcome>> ReverseForSaleAsync(
        Guid tenantId, Guid saleId, CancellationToken ct = default);

    /// <summary>Sum of available, unexpired entries' remaining amounts, less any outstanding
    /// clawback (FR-028).</summary>
    Task<decimal> GetUsableBalanceAsync(Guid tenantId, Guid customerId, CancellationToken ct = default);

    /// <summary>Same computation as <see cref="GetUsableBalanceAsync"/> for many customers at once,
    /// in two grouped queries instead of two per customer — for list endpoints. Customers with no
    /// cashback history are omitted from the result; treat a missing key as a zero balance.</summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetUsableBalancesAsync(
        Guid tenantId, IReadOnlyCollection<Guid> customerIds, CancellationToken ct = default);

    /// <summary>Full cashback history for a customer, newest-earned first (FR-021).</summary>
    Task<IReadOnlyList<CashbackEntry>> GetHistoryAsync(Guid tenantId, Guid customerId, CancellationToken ct = default);
}

/// <summary>Result of reversing one sale's cashback effects.</summary>
public record CashbackReversalOutcome(Guid SaleId, decimal ClawedBack, decimal RestoredToCustomer);
