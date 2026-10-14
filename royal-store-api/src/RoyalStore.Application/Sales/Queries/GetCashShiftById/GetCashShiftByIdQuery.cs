using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetCashShiftById;

public record GetCashShiftByIdQuery(Guid CashShiftId) : IRequest<Result<CashShiftDetailsResponse>>;
