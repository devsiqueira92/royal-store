namespace RoyalStore.Application.Sales.DTOs;

public record CloseCashShiftResponse(
    Guid Id,
    DateTime ClosedAt,
    decimal ExpectedEndingCash,
    decimal ActualEndingCash);
