using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.UpdateCustomer;

/// <summary><c>Addresses</c> carries only new and edited addresses (046): the ones left out stay as
/// they are, and none is deactivated, reactivated or removed here.</summary>
public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string? Phone,
    Guid? LoyaltyTierId,
    IReadOnlyList<CustomerAddressChangeRequest>? Addresses = null) : IRequest<Result<CustomerResponse>>;
