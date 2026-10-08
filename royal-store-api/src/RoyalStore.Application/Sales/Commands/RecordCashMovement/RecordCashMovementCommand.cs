using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.RecordCashMovement;

/// <summary>
/// Records a supplement (In) or withdrawal/sangria (Out) on the caller's open cash shift.
/// <see cref="Type"/> is always set by the route, never taken from the request body.
/// </summary>
public record RecordCashMovementCommand(MovementType Type, decimal Amount, string? Description)
    : IRequest<Result<CashMovementResponse>>;
