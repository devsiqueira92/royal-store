using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.DeactivateCustomerAddress;

public record DeactivateCustomerAddressCommand(Guid CustomerId, Guid AddressId)
    : IRequest<Result<CustomerAddressResponse>>;
