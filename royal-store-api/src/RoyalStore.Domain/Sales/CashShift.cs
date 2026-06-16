using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class CashShift : Entity, IAuditableEntity
{
    private readonly List<Sale> _sales = [];
    private readonly List<CashMovement> _movements = [];

    public Guid TenantId { get; private set; }
    public string? UserId { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public decimal StartingCash { get; private set; }
    public decimal? ExpectedEndingCash { get; private set; }
    public decimal? ActualEndingCash { get; private set; }

    public IReadOnlyList<Sale> Sales => _sales.AsReadOnly();
    public IReadOnlyList<CashMovement> Movements => _movements.AsReadOnly();

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private CashShift() : base(Guid.NewGuid()) { }

    private CashShift(Guid id, Guid tenantId, string? userId, DateTime openedAt, decimal startingCash)
        : base(id)
    {
        TenantId = tenantId;
        UserId = userId;
        OpenedAt = openedAt;
        StartingCash = startingCash;
    }

    public static Result<CashShift> Create(Guid id, Guid tenantId, string? userId, DateTime openedAt, decimal startingCash)
    {
        if (id == Guid.Empty)
            return Error.Validation("cashshift.id_required", "Cash shift ID is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("cashshift.tenant_required", "Tenant identifier is required.");

        if (startingCash < 0)
            return Error.Validation("cashshift.starting_cash_negative", "Starting cash cannot be negative.");

        return new CashShift(id, tenantId, userId, openedAt, startingCash);
    }

    public Result Close(decimal actualEndingCash, decimal expectedEndingCash)
    {
        if (ClosedAt is not null)
            return Result.Failure(Error.Conflict("cashshift.already_closed", "Cash shift is already closed."));

        ClosedAt = DateTime.UtcNow;
        ActualEndingCash = actualEndingCash;
        ExpectedEndingCash = expectedEndingCash;
        return Result.Success();
    }
}
