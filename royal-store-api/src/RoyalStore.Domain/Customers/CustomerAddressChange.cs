namespace RoyalStore.Domain.Customers;

/// <summary>One entry of an address list (046). A <c>null</c> <see cref="AddressId"/> is a new
/// address; otherwise it fully replaces that address's fields and default flag (045 FR-006).</summary>
public sealed record CustomerAddressChange(Guid? AddressId, PostalAddress Address, bool MarkAsDefault)
{
    public bool IsNew => AddressId is null;
}

/// <summary>What <see cref="Customer.ApplyAddressChanges"/> did, each list in the order sent.</summary>
public sealed record CustomerAddressChangeSet(
    IReadOnlyList<CustomerAddress> Added,
    IReadOnlyList<CustomerAddress> Updated);
