using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class CashShift : Entity, IAuditableEntity
{
    private readonly List<CashMovement> _movements = [];

    public Guid TenantId { get; private set; }
    public string? UserId { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public decimal StartingCash { get; private set; }
    public decimal? ExpectedEndingCash { get; private set; }
    public decimal? ActualEndingCash { get; private set; }

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

    public decimal SumSupplements() =>
        _movements.Where(m => m.Type == MovementType.In).Sum(m => m.Amount);

    public decimal SumWithdrawals() =>
        _movements.Where(m => m.Type == MovementType.Out).Sum(m => m.Amount);

    /// <summary>Cash that should physically be in the drawer: starting cash plus cash sales plus
    /// supplements minus withdrawals. Card and other non-cash sales never enter the drawer. Requires
    /// <see cref="Movements"/> to be loaded. May be negative when withdrawals exceed the drawer.</summary>
    public decimal CalculateExpectedCash(decimal cashSales) =>
        StartingCash + cashSales + SumSupplements() - SumWithdrawals();

    /// <summary>Closes the shift, recording the same expected cash the current-shift summary shows
    /// (<see cref="CalculateExpectedCash"/>), so the two figures never diverge.</summary>
    public Result Close(decimal actualEndingCash, decimal cashSales)
    {
        if (ClosedAt is not null)
            return Result.Failure(Error.Conflict("cashshift.already_closed", "Cash shift is already closed."));

        ClosedAt = DateTime.UtcNow;
        ActualEndingCash = actualEndingCash;
        ExpectedEndingCash = CalculateExpectedCash(cashSales);
        return Result.Success();
    }
}
