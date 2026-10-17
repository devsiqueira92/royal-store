using RoyalStore.Domain.Customers;

namespace RoyalStore.Application.Customers.DTOs;

public record CustomerAddressResponse(
    Guid Id,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Neighborhood,
    string? City,
    string? State,
    string? Complement,
    string? AdditionalInfo,
    bool IsDefaultDelivery,
    bool IsActive)
{
    public static CustomerAddressResponse From(CustomerAddress address) =>
        new(
            address.Id,
            address.ZipCode,
            address.Street,
            address.Number,
            address.Neighborhood,
            address.City,
            address.State,
            address.Complement,
            address.AdditionalInfo,
            address.IsDefaultDelivery,
            address.IsActive);
}
