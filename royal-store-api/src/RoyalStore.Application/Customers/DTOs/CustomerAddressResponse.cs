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

    /// <summary>Every address, active and inactive (045 FR-013): default first, then active, then
    /// oldest. Shared by every customer-shaped response so they all list addresses the same way.</summary>
    public static IReadOnlyList<CustomerAddressResponse> ListFrom(IEnumerable<CustomerAddress> addresses) =>
        addresses
            .OrderByDescending(a => a.IsDefaultDelivery)
            .ThenByDescending(a => a.IsActive)
            .ThenBy(a => a.CreatedAt)
            .Select(From)
            .ToList();
}
