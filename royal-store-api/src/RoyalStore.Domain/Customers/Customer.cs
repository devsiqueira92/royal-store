using RoyalStore.Domain.Common;
using RoyalStore.Domain.Loyalty;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Customers;

/// <summary>
/// A person identified at checkout. Tenant-scoped. Managed exclusively by Admin/Manager (FR-005);
/// sellers may read but not mutate (FR-006). Every customer belongs to exactly one
/// <see cref="Domain.Loyalty.LoyaltyTier"/>, defaulting to the tenant's Standard tier when none is
/// chosen (FR-013), and owns a cashback history read through <c>ICashbackLedger</c>.
/// </summary>
public sealed class Customer : Entity, IAuditableEntity
{
    private readonly List<CustomerAddress> _addresses = [];

    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public bool TermsAccepted { get; private set; }
    public Guid LoyaltyTierId { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>EF navigation to the customer's tier. Populated only when a query explicitly
    /// <c>Include</c>s it (e.g. the customer listing) — never assume it's loaded.</summary>
    public LoyaltyTier? LoyaltyTier { get; private set; }

    /// <summary>Delivery addresses. Loaded only by <c>GetByIdWithAddressesAsync</c> (or after
    /// <see cref="AddAddress"/>) — never assume it's populated otherwise.</summary>
    public IReadOnlyList<CustomerAddress> Addresses => _addresses.AsReadOnly();

    /// <summary>Optimistic concurrency token, bumped by every address change so two concurrent
    /// address edits can't both commit and break the one-default rule (045 research Decision 4).</summary>
    public int Version { get; private set; }

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Customer() : base(Guid.NewGuid()) { }

    private Customer(Guid id, Guid tenantId, string name, string document, string? phone,
        bool termsAccepted, Guid loyaltyTierId)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Document = document;
        Phone = phone;
        TermsAccepted = termsAccepted;
        LoyaltyTierId = loyaltyTierId;
        IsActive = true;
    }

    public static Result<Customer> Create(Guid tenantId, string name, string document,
        string? phone, bool termsAccepted, Guid loyaltyTierId)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("customer.tenant_required", "Tenant identifier is required.");

        var validation = ValidateNameAndPhone(name, phone);
        if (validation.IsFailure)
            return validation.Error;

        if (string.IsNullOrWhiteSpace(document))
            return Error.Validation("customer.document_required", "Document is required.");

        if (!DocumentValidator.IsValid(document))
            return Error.Validation("customer.document_invalid", "Document must be a valid CPF or CNPJ.");

        if (loyaltyTierId == Guid.Empty)
            return Error.Validation("customer.loyalty_tier_required", "Loyalty tier is required.");

        return new Customer(Guid.NewGuid(), tenantId, name.Trim(),
            DocumentValidator.Normalize(document), NormalizePhone(phone), termsAccepted, loyaltyTierId);
    }

    public Result Update(string name, string? phone)
    {
        var validation = ValidateNameAndPhone(name, phone);
        if (validation.IsFailure)
            return validation;

        Name = name.Trim();
        Phone = NormalizePhone(phone);
        return Result.Success();
    }

    public Result AcceptTerms()
    {
        TermsAccepted = true;
        return Result.Success();
    }

    /// <summary>Reassigns the customer's tier. Existence of <paramref name="loyaltyTierId"/> is the
    /// caller's responsibility to verify against the repository (FR-013) — mirrors how other entities
    /// in this codebase validate foreign references at the handler level, not inside the entity.</summary>
    public Result AssignTier(Guid loyaltyTierId)
    {
        if (loyaltyTierId == Guid.Empty)
            return Result.Failure(Error.Validation("customer.loyalty_tier_required", "Loyalty tier is required."));

        LoyaltyTierId = loyaltyTierId;
        return Result.Success();
    }

    /// <summary>Removal per FR-007: a deactivation, not an erasure — cashback history and past sales
    /// stay attributable. The customer simply stops appearing in listings and stops being selectable
    /// at checkout.</summary>
    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure(Error.Validation("customer.already_inactive", "Customer is already inactive."));

        IsActive = false;
        return Result.Success();
    }

    public Result Reactivate()
    {
        if (IsActive)
            return Result.Failure(Error.Validation("customer.already_active", "Customer is already active."));

        IsActive = true;
        return Result.Success();
    }

    /// <summary>Adds an active address. It becomes the default when marked, or when the customer has
    /// no active address — and so no default — yet (FR-010); either way the previous default is
    /// cleared in the same change (FR-008).</summary>
    public Result<CustomerAddress> AddAddress(Guid addressId, PostalAddress address, bool markAsDefault)
    {
        var becomesDefault = markAsDefault || !_addresses.Any(a => a.IsActive);

        var created = CustomerAddress.Create(addressId, TenantId, Id, address, becomesDefault);
        if (created.IsFailure)
            return created.Error;

        if (becomesDefault)
            ClearDefault();

        _addresses.Add(created.Value);
        Version++;
        return created.Value;
    }

    /// <summary>Full replacement of the address's fields and default flag (045 clarification). Marking
    /// it default clears any other default; unmarking the current default leaves none.</summary>
    public Result<CustomerAddress> UpdateAddress(Guid addressId, PostalAddress address, bool markAsDefault)
    {
        var found = FindAddress(addressId);
        if (found.IsFailure)
            return found.Error;

        var target = found.Value;
        if (markAsDefault && !target.IsActive)
            return InactiveCannotBeDefault;

        if (markAsDefault)
            ClearDefault(except: addressId);

        target.Update(address, markAsDefault);
        Version++;
        return target;
    }

    /// <summary>Deactivating the default leaves the customer with no default — no other address is
    /// promoted (FR-009).</summary>
    public Result<CustomerAddress> DeactivateAddress(Guid addressId)
    {
        var found = FindAddress(addressId);
        if (found.IsFailure)
            return found.Error;

        var target = found.Value;
        if (!target.IsActive)
            return Error.Validation("customer.address_already_inactive", "Address is already inactive.");

        target.Deactivate();
        Version++;
        return target;
    }

    /// <summary>The address comes back active but not default (FR-010).</summary>
    public Result<CustomerAddress> ReactivateAddress(Guid addressId)
    {
        var found = FindAddress(addressId);
        if (found.IsFailure)
            return found.Error;

        var target = found.Value;
        if (target.IsActive)
            return Error.Validation("customer.address_already_active", "Address is already active.");

        target.Reactivate();
        Version++;
        return target;
    }

    /// <summary>
    /// Applies an address list from one request (046). Every entry is validated first, so a failure
    /// leaves the customer untouched; then each kept entry is added or edited in order through
    /// <see cref="AddAddress"/>/<see cref="UpdateAddress"/>, which carry the default rule. Errors
    /// caused by one entry name its 1-based position in the list as sent, blank entries included.
    /// </summary>
    /// <param name="keepEmptyNewAddresses">The add action keeps new entries with every field blank
    /// (045 FR-011a); registration and update drop them, mark included (046 FR-012, FR-020).</param>
    public Result<CustomerAddressChangeSet> ApplyAddressChanges(
        IReadOnlyList<CustomerAddressChange> changes, bool keepEmptyNewAddresses = false)
    {
        var kept = changes
            .Select((change, index) => (Change: change, Position: index + 1))
            .Where(e => keepEmptyNewAddresses || !(e.Change.IsNew && e.Change.Address.IsEmpty))
            .ToList();

        var marked = kept.Where(e => e.Change.MarkAsDefault).Select(e => e.Position).ToList();
        if (marked.Count > 1)
            return Error.Validation("customer.address_multiple_defaults",
                $"Only one address can be the default delivery address (marked: {string.Join(", ", marked)}).");

        var seen = new HashSet<Guid>();
        foreach (var (change, position) in kept.Where(e => !e.Change.IsNew))
        {
            var addressId = change.AddressId!.Value;
            if (!seen.Add(addressId))
                return Error.Validation("customer.address_duplicated",
                    $"Address {position}: the same address appears more than once in the list.");

            var found = FindAddress(addressId);
            if (found.IsFailure)
                return AtPosition(found.Error, position);

            if (change.MarkAsDefault && !found.Value.IsActive)
                return AtPosition(InactiveCannotBeDefault, position);
        }

        var added = new List<CustomerAddress>();
        var updated = new List<CustomerAddress>();
        foreach (var (change, position) in kept)
        {
            var result = change.IsNew
                ? AddAddress(Guid.NewGuid(), change.Address, change.MarkAsDefault)
                : UpdateAddress(change.AddressId!.Value, change.Address, change.MarkAsDefault);
            if (result.IsFailure)
                return AtPosition(result.Error, position);

            (change.IsNew ? added : updated).Add(result.Value);
        }

        return new CustomerAddressChangeSet(added, updated);
    }

    private static readonly Error InactiveCannotBeDefault = Error.Validation(
        "customer.address_inactive_cannot_be_default",
        "An inactive address cannot be the default delivery address.");

    private static Error AtPosition(Error error, int position) =>
        error.WithMessage($"Address {position}: {error.Message}");

    private void ClearDefault(Guid? except = null)
    {
        foreach (var address in _addresses.Where(a => a.IsDefaultDelivery && a.Id != except))
            address.SetDefault(false);
    }

    private Result<CustomerAddress> FindAddress(Guid addressId)
    {
        var address = _addresses.FirstOrDefault(a => a.Id == addressId);
        return address is null
            ? Error.NotFound("customer.address_not_found", "Address not found for this customer.")
            : address;
    }

    private static Result ValidateNameAndPhone(string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("customer.name_required", "Name is required."));

        if (name.Trim().Length > 150)
            return Result.Failure(Error.Validation("customer.name_too_long", "Name must not exceed 150 characters."));

        if (!string.IsNullOrWhiteSpace(phone))
        {
            var normalized = NormalizePhone(phone);
            if (string.IsNullOrEmpty(normalized) || normalized.Length > 20)
                return Result.Failure(Error.Validation("customer.phone_invalid",
                    "Phone must contain digits only and must not exceed 20 characters."));
        }

        return Result.Success();
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return string.IsNullOrEmpty(digits) ? null : digits;
    }
}
