using RoyalStore.Domain.Payments;

namespace RoyalStore.Application.Abstractions.Data;

public interface IPaymentRepository
{
    Task AddRangeAsync(IEnumerable<Payment> payments, CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Payment>> GetAllAsync(CancellationToken ct = default);

    /// <summary>How many active payment methods the current tenant has — used to block a
    /// deactivation that would leave zero (FR-012).</summary>
    Task<int> CountActiveAsync(CancellationToken ct = default);

    Task UpdateAsync(Payment payment, CancellationToken ct = default);

    // No delete method — payment methods can never be removed (FR-004).
}
