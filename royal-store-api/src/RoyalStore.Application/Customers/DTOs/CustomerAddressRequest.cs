namespace RoyalStore.Application.Customers.DTOs;

/// <summary>One new address in a list (046), used by registration and the add action. Every field is
/// optional; normalization and limits live in <c>PostalAddress.Create</c>. When no entry is marked
/// and the customer has no active address, the first one becomes the default.</summary>
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
