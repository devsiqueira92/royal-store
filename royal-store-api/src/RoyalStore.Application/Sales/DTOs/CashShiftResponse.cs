using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record CashShiftResponse(
    Guid Id,
    string? UserId,
    decimal StartingCash,
    DateTime OpenedAt,
    decimal TotalSales,
    decimal TotalSupplements,
    decimal TotalWithdrawals,
    decimal ExpectedCash,
    decimal CashSales,
    decimal CreditCardSales,
    decimal DebitCardSales,
    decimal CardSales)
{
    public static CashShiftResponse From(CashShift shift, ShiftSalesTotals totals) =>
        new(shift.Id, shift.UserId, shift.StartingCash, shift.OpenedAt,
            totals.TotalSales,
            shift.SumSupplements(),
            shift.SumWithdrawals(),
            shift.CalculateExpectedCash(totals.CashSales),
            totals.CashSales,
            totals.CreditCardSales,
            totals.DebitCardSales,
            totals.CardSales);
}
