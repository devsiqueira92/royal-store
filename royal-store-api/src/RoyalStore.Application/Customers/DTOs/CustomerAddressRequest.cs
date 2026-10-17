namespace RoyalStore.Application.Customers.DTOs;

/// <summary>Address given while registering a customer. Every field is optional; normalization and
/// limits live in <c>PostalAddress.Create</c>. The first address is always the default, whatever
/// <c>IsDefaultDelivery</c> says.</summary>
public record CustomerAddressRequest(
    string? ZipCode,
    string? Street,
    string? Number,
    string? Neighborhood,
    string? City,
    string? State,
    string? Complement,
    string? AdditionalInfo,
    bool IsDefaultDelivery = false);
