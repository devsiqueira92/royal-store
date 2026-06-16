using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetCurrentCashShift;

public record GetCurrentCashShiftQuery : IRequest<Result<CashShiftResponse?>>;
