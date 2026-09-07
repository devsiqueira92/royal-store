using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Loyalty;

/// <summary>
/// Value the store reclaims when a purchase that had already granted and spent cashback is reversed
/// (FR-022b). Immutable once created — a record, not a mutable balance. Its
/// <c>(TenantId, SaleId)</c> uniqueness (enforced in <c>ClawbackConfiguration</c>) is what makes
/// reversing the same sale twice impossible (FR-022f): the second attempt is refused before a second
/// row is ever inserted.
/// </summary>
public sealed class Clawback : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid SaleId { get; private set; }
    public decimal Amount { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Clawback() : base(Guid.NewGuid()) { }

    private Clawback(Guid id, Guid tenantId, Guid customerId, Guid saleId, decimal amount)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        SaleId = saleId;
        Amount = amount;
    }

    public static Result<Clawback> Create(Guid tenantId, Guid customerId, Guid saleId, decimal amount)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("clawback.tenant_required", "Tenant identifier is required.");

        if (customerId == Guid.Empty)
            return Error.Validation("clawback.customer_required", "Customer identifier is required.");

        if (saleId == Guid.Empty)
            return Error.Validation("clawback.sale_required", "Sale identifier is required.");

        if (amount < 0)
            return Error.Validation("clawback.amount_negative", "Clawback amount cannot be negative.");

        return new Clawback(Guid.NewGuid(), tenantId, customerId, saleId, amount);
    }
}
