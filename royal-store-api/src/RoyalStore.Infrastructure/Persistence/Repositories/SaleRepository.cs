using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Sales;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class SaleRepository : ISaleRepository
{
    private readonly RoyalStoreContext _context;

    public SaleRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Sale sale, CancellationToken ct = default) =>
        await _context.Sales.AddAsync(sale, ct);

    public async Task AddItemsAsync(IEnumerable<SaleItem> items, CancellationToken ct = default) =>
        await _context.SaleItems.AddRangeAsync(items, ct);

    public async Task AddPaymentsAsync(IEnumerable<SalePayment> payments, CancellationToken ct = default) =>
        await _context.SalePayments.AddRangeAsync(payments, ct);

    public async Task<HashSet<Guid>> GetExistingIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default) =>
        new HashSet<Guid>(await _context.Sales
            .Where(s => ids.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync(ct));
}
