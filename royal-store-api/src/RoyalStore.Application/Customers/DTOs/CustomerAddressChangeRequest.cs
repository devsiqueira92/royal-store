namespace RoyalStore.Application.Customers.DTOs;

/// <summary>One entry of the customer update's address list (046). Without <c>Id</c> it is a new
/// address; with <c>Id</c> it fully replaces that address — a field left out becomes empty and a
/// missing <c>IsDefaultDelivery</c> means "not default" (045 FR-006). Field rules live in
/// <c>PostalAddress.Create</c>.</summary>
public record CustomerAddressChangeRequest(
    Guid? Id,
    string? ZipCode,
    string? Street,
    string? Number,
    string? Neighborhood,
    string? City,
    string? State,
    string? Complement,
    string? AdditionalInfo,
    bool IsDefaultDelivery = false);
