using MediatR;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.AddCustomerAddress;

public record AddCustomerAddressCommand(
    Guid CustomerId,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Neighborhood,
    string? City,
    string? State,
    string? Complement,
    string? AdditionalInfo,
    bool IsDefaultDelivery = false) : IRequest<Result<CustomerAddressResponse>>;
