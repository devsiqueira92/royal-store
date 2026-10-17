using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.UpdateCustomerAddress;

/// <summary>Full replacement: a field left out becomes empty, and a missing
/// <c>IsDefaultDelivery</c> means "not default" (045 clarification).</summary>
public record UpdateCustomerAddressCommand(
    Guid CustomerId,
    Guid AddressId,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Neighborhood,
    string? City,
    string? State,
    string? Complement,
    string? AdditionalInfo,
    bool IsDefaultDelivery = false) : IRequest<Result<CustomerAddressResponse>>;
