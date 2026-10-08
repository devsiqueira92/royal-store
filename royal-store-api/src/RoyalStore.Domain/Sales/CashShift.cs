using System.Globalization;
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

    /// <summary>Optimistic concurrency token, bumped by every state transition so concurrent
    /// movements or a movement racing a close cannot both commit against a stale drawer.</summary>
    public int Version { get; private set; }

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

    /// <summary>Records a withdrawal (sangria). Rejected when it exceeds the expected cash in the drawer
    /// (<see cref="CalculateExpectedCash"/>); a withdrawal of exactly the expected cash is allowed.</summary>
    public Result<CashMovement> RecordWithdrawal(Guid movementId, string? userId, decimal amount,
        string? description, DateTime movedAt, decimal cashSales)
    {
        if (ClosedAt is not null)
            return Error.Conflict("cashshift.already_closed", "Cash shift is already closed.");

        var expectedCash = CalculateExpectedCash(cashSales);
        if (amount > expectedCash)
            return Error.Conflict("cashshift.insufficient_cash",
                $"Withdrawal amount exceeds the expected cash in the drawer ({expectedCash.ToString("F2", CultureInfo.InvariantCulture)}).");

        return AddMovement(movementId, userId, MovementType.Out, amount, description, movedAt);
    }

    /// <summary>Records a supplement (cash added to the drawer). Has no upper limit.</summary>
    public Result<CashMovement> RecordSupplement(Guid movementId, string? userId, decimal amount,
        string? description, DateTime movedAt) =>
        AddMovement(movementId, userId, MovementType.In, amount, description, movedAt);

    /// <summary>Records a completed sale in the movement history: the amount actually paid (after all
    /// discounts, without cashback) and a <c>#{saleId} - {methods}</c> description. Informational only —
    /// <see cref="SumSupplements"/>/<see cref="SumWithdrawals"/> ignore it, since cash sales already reach
    /// <see cref="CalculateExpectedCash"/> from the sales themselves. Returns <c>null</c> for a zero-amount sale.</summary>
    public Result<CashMovement?> RecordSale(Guid movementId, Sale sale, IReadOnlyList<string> paymentMethodNames)
    {
        if (sale.CashShiftId != Id)
            return Error.Validation("cashshift.sale_shift_mismatch", "The sale does not belong to this cash shift.");

        if (sale.TotalAmount == 0)
            return Result<CashMovement?>.Success(null);

        var names = paymentMethodNames.Distinct(StringComparer.Ordinal).ToList();
        var description = names.Count > 0 ? $"#{sale.Id} - {string.Join(", ", names)}" : $"#{sale.Id}";
        if (description.Length > CashMovement.DescriptionMaxLength)
            description = description[..(CashMovement.DescriptionMaxLength - 1)] + "…";

        var movementResult = AddMovement(movementId, sale.UserId, MovementType.Sale, sale.TotalAmount, description, sale.SoldAt);
        if (movementResult.IsFailure)
            return movementResult.Error;

        return movementResult.Value;
    }

    /// <summary>Closes the shift, recording the same expected cash the current-shift summary shows
    /// (<see cref="CalculateExpectedCash"/>), so the two figures never diverge.</summary>
    public Result Close(decimal actualEndingCash, decimal cashSales)
    {
        if (ClosedAt is not null)
            return Result.Failure(Error.Conflict("cashshift.already_closed", "Cash shift is already closed."));

        ClosedAt = DateTime.UtcNow;
        ActualEndingCash = actualEndingCash;
        ExpectedEndingCash = CalculateExpectedCash(cashSales);
        Version++;
        return Result.Success();
    }

    private Result<CashMovement> AddMovement(Guid movementId, string? userId, MovementType type,
        decimal amount, string? description, DateTime movedAt)
    {
        if (ClosedAt is not null)
            return Error.Conflict("cashshift.already_closed", "Cash shift is already closed.");

        var movementResult = CashMovement.Create(movementId, TenantId, Id, userId, type, amount, description, movedAt, movedAt);
        if (movementResult.IsFailure)
            return movementResult.Error;

        _movements.Add(movementResult.Value);
        Version++;
        return movementResult.Value;
    }
}
