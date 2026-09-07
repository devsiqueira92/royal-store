using MediatR;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.ReactivateCustomer;

public record ReactivateCustomerCommand(Guid Id) : IRequest<Result>;
