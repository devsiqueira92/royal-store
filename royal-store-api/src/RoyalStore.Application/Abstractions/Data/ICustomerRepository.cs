using RoyalStore.Domain.Customers;

namespace RoyalStore.Application.Abstractions.Data;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tracked, with <c>LoyaltyTier</c> and <c>Addresses</c> loaded. Kept separate from
    /// <see cref="GetByIdAsync"/> so ordinary customer edits — which call <see cref="UpdateAsync"/> on
    /// the whole graph — never touch (and re-stamp) the addresses.</summary>
    Task<Customer?> GetByIdWithAddressesAsync(Guid id, CancellationToken ct = default);

    Task AddAddressAsync(CustomerAddress address, CancellationToken ct = default);
    Task<Customer?> GetByDocumentAsync(Guid tenantId, string normalizedDocument, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);

    /// <summary>
    /// Active customers, ordered by name, with <c>LoyaltyTier</c> loaded and untracked. Each filter
    /// is optional and they combine with OR: <paramref name="normalizedName"/> is a substring of the
    /// normalized name (<see cref="Common.TextSearchNormalizer"/>); <paramref name="digits"/> equals
    /// the full document or phone (both encrypted, so only an exact match through their blind index).
    /// With no filter at all, the first <paramref name="take"/> customers by name.
    /// </summary>
    Task<IReadOnlyList<Customer>> SearchActiveAsync(
        string? normalizedName, string? digits, int take, CancellationToken ct = default);
    Task<bool> ExistsByDocumentAsync(string normalizedDocument, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(Customer customer, CancellationToken ct = default);
}
