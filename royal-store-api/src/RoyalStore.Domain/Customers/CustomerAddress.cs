using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Customers;

/// <summary>
/// A place where a customer receives deliveries. A child of the <see cref="Customer"/> aggregate:
/// every mutator is <c>internal</c>, so the one-default and inactive-is-never-default rules can only
/// be applied through <see cref="Customer"/>'s address methods. Never deleted, only deactivated.
/// </summary>
public sealed class CustomerAddress : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string? ZipCode { get; private set; }
    public string? Street { get; private set; }
    public string? Number { get; private set; }
    public string? Neighborhood { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? Complement { get; private set; }
    public string? AdditionalInfo { get; private set; }
    public bool IsDefaultDelivery { get; private set; }
    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private CustomerAddress() : base(Guid.NewGuid()) { }

    private CustomerAddress(Guid id, Guid tenantId, Guid customerId, PostalAddress address, bool isDefaultDelivery)
        : base(id)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        Apply(address);
        IsDefaultDelivery = isDefaultDelivery;
        IsActive = true;
    }

    internal static Result<CustomerAddress> Create(Guid id, Guid tenantId, Guid customerId,
        PostalAddress address, bool isDefaultDelivery)
    {
        if (id == Guid.Empty)
            return Error.Validation("customer.address_id_required", "Address identifier is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("customer.tenant_required", "Tenant identifier is required.");

        if (customerId == Guid.Empty)
            return Error.Validation("customer.address_customer_required", "Customer identifier is required.");

        return new CustomerAddress(id, tenantId, customerId, address, isDefaultDelivery);
    }

    /// <summary>Full replacement (045 clarification): every field takes the given value, so a field
    /// left out of the edit becomes empty.</summary>
    internal void Update(PostalAddress address, bool isDefaultDelivery)
    {
        Apply(address);
        IsDefaultDelivery = isDefaultDelivery;
    }

    internal void SetDefault(bool isDefault) => IsDefaultDelivery = isDefault;

    /// <summary>An inactive address is never the default (FR-009).</summary>
    internal void Deactivate()
    {
        IsActive = false;
        IsDefaultDelivery = false;
    }

    internal void Reactivate() => IsActive = true;

    private void Apply(PostalAddress address)
    {
        ZipCode = address.ZipCode;
        Street = address.Street;
        Number = address.Number;
        Neighborhood = address.Neighborhood;
        City = address.City;
        State = address.State;
        Complement = address.Complement;
        AdditionalInfo = address.AdditionalInfo;
    }
}
