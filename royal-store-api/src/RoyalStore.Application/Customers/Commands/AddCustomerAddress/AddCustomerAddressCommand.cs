using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.AddCustomerAddress;

/// <summary>One or more new addresses (046), all saved or none. Returns the created addresses in the
/// order sent.</summary>
public record AddCustomerAddressCommand(
    Guid CustomerId,
    IReadOnlyList<CustomerAddressRequest> Addresses) : IRequest<Result<IReadOnlyList<CustomerAddressResponse>>>;
