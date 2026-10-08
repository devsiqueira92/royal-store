using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetCashShiftMovements;

public record GetCashShiftMovementsQuery(Guid CashShiftId) : IRequest<Result<IReadOnlyList<CashMovementResponse>>>;
