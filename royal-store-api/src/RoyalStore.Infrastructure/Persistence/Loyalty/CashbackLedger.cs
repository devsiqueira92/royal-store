using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Loyalty;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Loyalty;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Loyalty;

/// <summary>
/// Implements <see cref="ICashbackLedger"/> directly against <see cref="RoyalStoreContext"/> — not
/// through the plain CRUD repositories — because every operation here is a concurrency-sensitive
/// read-modify-write sequence (research.md Decisions 2 and 5). Callers control the transaction and the
/// final <c>SaveChangesAsync</c>; this class only tracks changes on entities, it never saves.
/// </summary>
public sealed class CashbackLedger : ICashbackLedger
{
    private readonly RoyalStoreContext _context;

    public CashbackLedger(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task<Result<decimal>> RedeemAsync(
        Guid tenantId, Guid customerId, decimal requestedAmount, Guid saleId, CancellationToken ct = default)
    {
        if (requestedAmount <= 0)
            return Result<decimal>.Success(0m);

        var now = DateTime.UtcNow;

        // "Effectively available" expressed as a queryable predicate: stored Available AND not yet
        // expired — the same two conditions CashbackEntryStateResolver applies in memory.
        var entries = await _context.CashbackEntries
            .Where(e => e.TenantId == tenantId && e.CustomerId == customerId
                && e.State == CashbackEntryState.Available && e.ExpiresAt >= now)
            .OrderBy(e => e.ExpiresAt)
            .ToListAsync(ct);

        var grossAvailable = entries.Sum(e => e.RemainingAmount);
        var outstandingClawback = await _context.Clawbacks
            .Where(c => c.TenantId == tenantId && c.CustomerId == customerId)
            .SumAsync(c => c.Amount, ct);

        var netAvailable = Math.Max(0m, grossAvailable - outstandingClawback);
        var amountToApply = Math.Min(requestedAmount, netAvailable);

        if (amountToApply <= 0)
            return Result<decimal>.Success(0m);

        var remaining = amountToApply;
        foreach (var entry in entries)
        {
            if (remaining <= 0)
                break;

            var take = Math.Min(entry.RemainingAmount, remaining);
            if (take <= 0)
                continue;

            var consumeResult = entry.Consume(take);
            if (consumeResult.IsFailure)
                return Result<decimal>.Failure(consumeResult.Error);

            var redemptionResult = CashbackRedemption.Create(tenantId, customerId, saleId, entry.Id, take);
            if (redemptionResult.IsFailure)
                return Result<decimal>.Failure(redemptionResult.Error);

            await _context.CashbackRedemptions.AddAsync(redemptionResult.Value, ct);
            remaining -= take;
        }

        return Result<decimal>.Success(amountToApply);
    }

    public async Task<Result<decimal>> AccrueAsync(
        Guid tenantId, Guid customerId, Guid saleId, decimal earningBase, DateTime earnedAt, CancellationToken ct = default)
    {
        if (earningBase <= 0)
            return Result<decimal>.Success(0m);

        var customer = await _context.Customers.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Id == customerId, ct);
        if (customer is null)
            return Result<decimal>.Failure(Error.NotFound("cashbackledger.customer_not_found", "Customer not found."));

        var tier = await _context.LoyaltyTiers.FirstOrDefaultAsync(
            t => t.TenantId == tenantId && t.Id == customer.LoyaltyTierId, ct);
        if (tier is null)
            return Result<decimal>.Failure(Error.NotFound("cashbackledger.tier_not_found", "Customer's loyalty tier not found."));

        var grantedAmount = Math.Round(earningBase * tier.CashbackPercentage / 100m, 2, MidpointRounding.AwayFromZero);
        if (grantedAmount <= 0)
            return Result<decimal>.Success(0m);

        var entryResult = CashbackEntry.Create(tenantId, customerId, saleId, grantedAmount, earnedAt, tier.CashbackValidityDays);
        if (entryResult.IsFailure)
            return Result<decimal>.Failure(entryResult.Error);

        await _context.CashbackEntries.AddAsync(entryResult.Value, ct);
        return Result<decimal>.Success(grantedAmount);
    }

    public async Task<Result<CashbackReversalOutcome>> ReverseForSaleAsync(
        Guid tenantId, Guid saleId, CancellationToken ct = default)
    {
        var alreadyReversed = await _context.Clawbacks.AnyAsync(
            c => c.TenantId == tenantId && c.SaleId == saleId, ct);
        if (alreadyReversed)
            return Result<CashbackReversalOutcome>.Failure(
                Error.Conflict("sale.already_reversed", "This sale's cashback has already been reversed."));

        var sale = await _context.Sales.FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Id == saleId, ct);
        if (sale is null || sale.CustomerId is null)
            return Result<CashbackReversalOutcome>.Failure(
                Error.NotFound("sale.not_found_or_unidentified", "Sale not found, or was unidentified — nothing to reverse."));

        var customerId = sale.CustomerId.Value;
        var now = DateTime.UtcNow;

        // 1. Claw back what this sale earned (FR-022a/b): fully revoke the entry it granted,
        //    recording as "clawed back" only the portion that had already been spent.
        var clawedBack = 0m;
        var earnedEntry = await _context.CashbackEntries.FirstOrDefaultAsync(
            e => e.TenantId == tenantId && e.SaleId == saleId, ct);
        if (earnedEntry is not null)
        {
            var alreadySpent = earnedEntry.GrantedAmount - earnedEntry.RemainingAmount;
            var revokeResult = earnedEntry.Revoke();
            if (revokeResult.IsFailure)
                return Result<CashbackReversalOutcome>.Failure(revokeResult.Error);

            clawedBack = alreadySpent;
        }

        // 2. Restore what this sale redeemed (FR-022d), entry by entry, so each portion carries its
        //    own originating entry's expiry. An entry already expired by now is left alone — no
        //    spendable value is handed back for it (FR-022e).
        var restoredToCustomer = 0m;
        var redemptions = await _context.CashbackRedemptions
            .Where(r => r.TenantId == tenantId && r.SaleId == saleId)
            .ToListAsync(ct);

        foreach (var redemption in redemptions)
        {
            var sourceEntry = await _context.CashbackEntries.FirstOrDefaultAsync(
                e => e.TenantId == tenantId && e.Id == redemption.CashbackEntryId, ct);
            if (sourceEntry is null || sourceEntry.ExpiresAt < now)
                continue;

            var restoreResult = sourceEntry.Restore(redemption.Amount, sourceEntry.ExpiresAt);
            if (restoreResult.IsFailure)
                return Result<CashbackReversalOutcome>.Failure(restoreResult.Error);

            restoredToCustomer += redemption.Amount;
        }

        // 3. Record the clawback — created even when zero, so the (TenantId, SaleId) uniqueness
        //    guard makes a second reversal attempt impossible (FR-022f).
        var clawbackResult = Clawback.Create(tenantId, customerId, saleId, clawedBack);
        if (clawbackResult.IsFailure)
            return Result<CashbackReversalOutcome>.Failure(clawbackResult.Error);

        await _context.Clawbacks.AddAsync(clawbackResult.Value, ct);

        return Result<CashbackReversalOutcome>.Success(
            new CashbackReversalOutcome(saleId, clawedBack, restoredToCustomer));
    }

    public async Task<decimal> GetUsableBalanceAsync(Guid tenantId, Guid customerId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var grossAvailable = await _context.CashbackEntries
            .Where(e => e.TenantId == tenantId && e.CustomerId == customerId
                && e.State == CashbackEntryState.Available && e.ExpiresAt >= now)
            .SumAsync(e => e.RemainingAmount, ct);

        var outstandingClawback = await _context.Clawbacks
            .Where(c => c.TenantId == tenantId && c.CustomerId == customerId)
            .SumAsync(c => c.Amount, ct);

        // Deliberately not clamped to zero here — FR-022c requires the balance to be able to show
        // negative, so staff can see a customer owes an outstanding clawback.
        return grossAvailable - outstandingClawback;
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetUsableBalancesAsync(
        Guid tenantId, IReadOnlyCollection<Guid> customerIds, CancellationToken ct = default)
    {
        if (customerIds.Count == 0)
            return new Dictionary<Guid, decimal>();

        var now = DateTime.UtcNow;

        var grossAvailable = await _context.CashbackEntries
            .Where(e => e.TenantId == tenantId && customerIds.Contains(e.CustomerId)
                && e.State == CashbackEntryState.Available && e.ExpiresAt >= now)
            .GroupBy(e => e.CustomerId)
            .Select(g => new { CustomerId = g.Key, Amount = g.Sum(e => e.RemainingAmount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Amount, ct);

        var outstandingClawback = await _context.Clawbacks
            .Where(c => c.TenantId == tenantId && customerIds.Contains(c.CustomerId))
            .GroupBy(c => c.CustomerId)
            .Select(g => new { CustomerId = g.Key, Amount = g.Sum(c => c.Amount) })
            .ToDictionaryAsync(x => x.CustomerId, x => x.Amount, ct);

        var result = new Dictionary<Guid, decimal>(customerIds.Count);
        foreach (var customerId in customerIds)
        {
            var gross = grossAvailable.GetValueOrDefault(customerId);
            var clawback = outstandingClawback.GetValueOrDefault(customerId);
            if (gross != 0m || clawback != 0m)
                result[customerId] = gross - clawback;
        }

        return result;
    }

    public async Task<IReadOnlyList<CashbackEntry>> GetHistoryAsync(
        Guid tenantId, Guid customerId, CancellationToken ct = default) =>
        await _context.CashbackEntries
            .Where(e => e.TenantId == tenantId && e.CustomerId == customerId)
            .OrderByDescending(e => e.EarnedAt)
            .ToListAsync(ct);
}
