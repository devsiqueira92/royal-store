using RoyalStore.Domain.Customers;

namespace RoyalStore.Application.Abstractions.Data;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByDocumentAsync(Guid tenantId, string normalizedDocument, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default);
    Task<bool> ExistsByDocumentAsync(string normalizedDocument, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(Customer customer, CancellationToken ct = default);
}
