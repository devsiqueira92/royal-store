using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Customers;

namespace RoyalStore.Application.Customers.Commands;

/// <summary>Turns request address entries into domain changes, naming the entry's 1-based position
/// in any field error (046 FR-003). List rules stay in <c>Customer.ApplyAddressChanges</c>.</summary>
internal static class CustomerAddressChanges
{
    public static Result<IReadOnlyList<CustomerAddressChange>> From(IReadOnlyList<CustomerAddressRequest>? requests) =>
        Build(requests,
            _ => null,
            r => PostalAddress.Create(r.ZipCode, r.Street, r.Number, r.Neighborhood, r.City, r.State,
                r.Complement, r.AdditionalInfo),
            r => r.IsDefaultDelivery);

    public static Result<IReadOnlyList<CustomerAddressChange>> From(IReadOnlyList<CustomerAddressChangeRequest>? requests) =>
        Build(requests,
            r => r.Id,
            r => PostalAddress.Create(r.ZipCode, r.Street, r.Number, r.Neighborhood, r.City, r.State,
                r.Complement, r.AdditionalInfo),
            r => r.IsDefaultDelivery);

    private static Result<IReadOnlyList<CustomerAddressChange>> Build<T>(
        IReadOnlyList<T>? requests,
        Func<T, Guid?> idOf,
        Func<T, Result<PostalAddress>> postalOf,
        Func<T, bool> markOf)
    {
        var changes = new List<CustomerAddressChange>(requests?.Count ?? 0);
        if (requests is null)
            return Result<IReadOnlyList<CustomerAddressChange>>.Success(changes);

        for (var i = 0; i < requests.Count; i++)
        {
            var postal = postalOf(requests[i]);
            if (postal.IsFailure)
                return postal.Error.WithMessage($"Address {i + 1}: {postal.Error.Message}");

            changes.Add(new CustomerAddressChange(idOf(requests[i]), postal.Value, markOf(requests[i])));
        }

        return Result<IReadOnlyList<CustomerAddressChange>>.Success(changes);
    }
}
