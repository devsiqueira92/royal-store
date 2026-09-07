using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.GetCustomers;

public record GetCustomersQuery : IRequest<Result<IReadOnlyList<CustomerResponse>>>;
