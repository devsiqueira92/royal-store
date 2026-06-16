using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record CashShiftResponse(Guid Id, string? UserId, decimal StartingCash, DateTime OpenedAt)
{
    public static CashShiftResponse From(CashShift shift) =>
        new(shift.Id, shift.UserId, shift.StartingCash, shift.OpenedAt);
}
