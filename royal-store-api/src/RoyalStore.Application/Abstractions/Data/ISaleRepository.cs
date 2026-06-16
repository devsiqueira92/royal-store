using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Abstractions.Data;

public interface ISaleRepository
{
    Task AddAsync(Sale sale, CancellationToken ct = default);
    Task AddItemsAsync(IEnumerable<SaleItem> items, CancellationToken ct = default);
    Task AddPaymentsAsync(IEnumerable<SalePayment> payments, CancellationToken ct = default);
    Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
