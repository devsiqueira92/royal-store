using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string Name,
    string? Document,
    string? Phone,
    Guid? LoyaltyTierId,
    DateOnly? BirthDate = null,
    IReadOnlyList<CustomerAddressRequest>? Addresses = null) : IRequest<Result<CustomerResponse>>;
