namespace RoyalStore.Application.Abstractions.Data;

public sealed record CashShiftWithMovementTotals(
    Guid Id,
    string? UserId,
    string? UserName,
    decimal StartingCash,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal TotalSupplements,
    decimal TotalWithdrawals,
    int MovementCount);
