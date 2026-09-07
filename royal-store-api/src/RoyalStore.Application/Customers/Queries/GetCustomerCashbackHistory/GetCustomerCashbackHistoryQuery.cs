using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.GetCustomerCashbackHistory;

public record GetCustomerCashbackHistoryQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<CashbackEntryResponse>>>;
