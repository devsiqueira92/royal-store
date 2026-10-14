using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record CashShiftListItemResponse(
    Guid Id,
    string? UserId,
    string? UserName,
    decimal StartingCash,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal TotalSales,
    decimal TotalSupplements,
    decimal TotalWithdrawals,
    decimal CashSales,
    decimal CreditCardSales,
    decimal CardSales,
    int SalesCount,
    int NumberOfMovements)
{
    public static CashShiftListItemResponse From(CashShiftWithMovementTotals shift, ShiftSalesTotals totals) =>
        new(shift.Id, shift.UserId, shift.UserName, shift.StartingCash, shift.OpenedAt, shift.ClosedAt,
            totals.TotalSales,
            shift.TotalSupplements,
            shift.TotalWithdrawals,
            totals.CashSales,
            totals.CreditCardSales,
            totals.CardSales,
            totals.SalesCount,
            shift.MovementCount);
}
