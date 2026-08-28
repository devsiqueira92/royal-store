using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Abstractions.Data;

public interface ISellerRepository
{
    Task AddAsync(Seller seller, CancellationToken ct = default);
    Task<Seller?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Seller?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<Seller>> GetAllAsync(bool? isActive, CancellationToken ct = default);
    Task<bool> ExistsByCpfAsync(string cpf, Guid? excludeId, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken ct = default);
    Task UpdateAsync(Seller seller, CancellationToken ct = default);
}
