using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record CashShiftDetailsResponse(
    Guid Id,
    string? UserId,
    string? UserName,
    decimal StartingCash,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal TotalSales,
    decimal TotalSupplements,
    decimal TotalWithdrawals,
    decimal ExpectedCash,
    decimal CashSales,
    decimal CreditCardSales,
    decimal DebitCardSales,
    decimal CardSales,
    int SalesCount,
    decimal? ExpectedEndingCash,
    decimal? ActualEndingCash)
{
    public static CashShiftDetailsResponse From(CashShift shift, string? userName, ShiftSalesTotals totals) =>
        new(shift.Id, shift.UserId, userName, shift.StartingCash, shift.OpenedAt, shift.ClosedAt,
            totals.TotalSales,
            shift.SumSupplements(),
            shift.SumWithdrawals(),
            shift.CalculateExpectedCash(totals.CashSales),
            totals.CashSales,
            totals.CreditCardSales,
            totals.DebitCardSales,
            totals.CardSales,
            totals.SalesCount,
            shift.ExpectedEndingCash,
            shift.ActualEndingCash);
}
