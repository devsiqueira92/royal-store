using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record CashMovementResponse(
    Guid Id,
    Guid CashShiftId,
    string Type,
    decimal Amount,
    string? Description,
    string? UserId,
    DateTime MovedAt)
{
    public static CashMovementResponse From(CashMovement movement) =>
        new(movement.Id,
            movement.CashShiftId,
            movement.Type switch
            {
                MovementType.In => "supplement",
                MovementType.Out => "withdrawal",
                MovementType.Sale => "sale",
                _ => movement.Type.ToString().ToLowerInvariant(),
            },
            movement.Amount,
            movement.Description,
            movement.UserId,
            movement.MovedAt);
}
