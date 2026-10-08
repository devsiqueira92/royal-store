using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class CashMovement : Entity
{
    public const int DescriptionMaxLength = 250;

    public Guid TenantId { get; private set; }
    public Guid CashShiftId { get; private set; }
    public string? UserId { get; private set; }
    public MovementType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public DateTime MovedAt { get; private set; }
    public DateTime SyncedAt { get; private set; }

    private CashMovement() : base(Guid.NewGuid()) { }

    private CashMovement(Guid id, Guid tenantId, Guid cashShiftId, string? userId, MovementType type,
        decimal amount, string? description, DateTime movedAt, DateTime syncedAt)
        : base(id)
    {
        TenantId = tenantId;
        CashShiftId = cashShiftId;
        UserId = userId;
        Type = type;
        Amount = amount;
        Description = description;
        MovedAt = movedAt;
        SyncedAt = syncedAt;
    }

    public static Result<CashMovement> Create(Guid id, Guid tenantId, Guid cashShiftId, string? userId,
        MovementType type, decimal amount, string? description, DateTime movedAt, DateTime syncedAt)
    {
        if (id == Guid.Empty)
            return Error.Validation("cashmovement.id_required", "Cash movement ID is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("cashmovement.tenant_required", "Tenant identifier is required.");

        if (cashShiftId == Guid.Empty)
            return Error.Validation("cashmovement.cashshift_required", "Cash shift identifier is required.");

        if (!Enum.IsDefined(type))
            return Error.Validation("cashmovement.type_invalid", "Movement type is invalid.");

        if (amount <= 0)
            return Error.Validation("cashmovement.amount_invalid", "Movement amount must be greater than zero.");

        if (amount != decimal.Round(amount, 2))
            return Error.Validation("cashmovement.amount_precision", "Movement amount must have at most two decimal places.");

        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalizedDescription?.Length > DescriptionMaxLength)
            return Error.Validation("cashmovement.description_too_long", "Movement description must not exceed 250 characters.");

        return new CashMovement(id, tenantId, cashShiftId, userId, type, amount, normalizedDescription, movedAt, syncedAt);
    }
}
