using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Application.Abstractions.Data;

public interface ISupplierRepository
{
    Task AddAsync(Supplier supplier, CancellationToken ct = default);
    Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetAllAsync(bool? isActive, CancellationToken ct = default);
    Task<bool> ExistsByCnpjAsync(string cnpj, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(Supplier supplier, CancellationToken ct = default);
}
