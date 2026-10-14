using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Abstractions.Data;

public sealed record CashShiftWithOperatorName(CashShift Shift, string? OperatorName);
