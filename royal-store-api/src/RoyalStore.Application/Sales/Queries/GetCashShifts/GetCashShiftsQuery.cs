using MediatR;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetCashShifts;

public record GetCashShiftsQuery : IRequest<Result<IReadOnlyList<CashShiftListItemResponse>>>;
