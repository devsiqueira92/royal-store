using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.ReactivateCustomerAddress;

public record ReactivateCustomerAddressCommand(Guid CustomerId, Guid AddressId)
    : IRequest<Result<CustomerAddressResponse>>;
