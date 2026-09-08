using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.GetCustomerCashbackBalance;

public record GetCustomerCashbackBalanceQuery(Guid CustomerId) : IRequest<Result<decimal>>;
