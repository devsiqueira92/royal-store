using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Loyalty;

/// <summary>
/// One draw against one <see cref="CashbackEntry"/> by one sale. <see cref="Sale.CashbackApplied"/>
/// records the total a sale redeemed; this record exists so reversal (FR-022d) knows exactly which
/// entries that total was drawn from and — critically — each one's own expiry, since a reversal
/// restores value "carrying the expiry date of the entry it came from," and one redemption can span
/// several entries (FR-028a). A plain child record, not <c>IAuditableEntity</c> — same treatment as
/// <c>SaleItem</c>/<c>SalePayment</c>.
/// </summary>
public sealed class CashbackRedemption : Entity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid SaleId { get; private set; }
    public Guid CashbackEntryId { get; private set; }
    public decimal Amount { get; private set; }

    private CashbackRedemption() : base(Guid.NewGuid()) { }

    private CashbackRedemption(Guid id, Guid tenantId, Guid customerId, Guid saleId, Guid cashbackEntryId, decimal amount)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        SaleId = saleId;
        CashbackEntryId = cashbackEntryId;
        Amount = amount;
    }

    public static Result<CashbackRedemption> Create(
        Guid tenantId, Guid customerId, Guid saleId, Guid cashbackEntryId, decimal amount)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("cashbackredemption.tenant_required", "Tenant identifier is required.");

        if (customerId == Guid.Empty)
            return Error.Validation("cashbackredemption.customer_required", "Customer identifier is required.");

        if (saleId == Guid.Empty)
            return Error.Validation("cashbackredemption.sale_required", "Sale identifier is required.");

        if (cashbackEntryId == Guid.Empty)
            return Error.Validation("cashbackredemption.entry_required", "Cashback entry identifier is required.");

        if (amount <= 0)
            return Error.Validation("cashbackredemption.amount_invalid", "Amount must be greater than zero.");

        return new CashbackRedemption(Guid.NewGuid(), tenantId, customerId, saleId, cashbackEntryId, amount);
    }
}
