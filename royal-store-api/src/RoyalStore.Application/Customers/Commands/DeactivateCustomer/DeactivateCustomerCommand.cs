using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.DeactivateCustomer;

public record DeactivateCustomerCommand(Guid Id) : IRequest<Result>;
