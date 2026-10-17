using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Customers;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly RoyalStoreContext _context;

    public CustomerRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Customer customer, CancellationToken ct = default) =>
        await _context.Customers.AddAsync(customer, ct);

    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Customers
            .Include(c => c.LoyaltyTier)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Customer?> GetByIdWithAddressesAsync(Guid id, CancellationToken ct = default) =>
        await _context.Customers
            .Include(c => c.LoyaltyTier)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    // Explicit Add, not just Customer.AddAddress: a client-assigned Guid key found through a
    // navigation is tracked as Modified, not Added (040 research Decision 3).
    public async Task AddAddressAsync(CustomerAddress address, CancellationToken ct = default) =>
        await _context.CustomerAddresses.AddAsync(address, ct);

    public async Task<Customer?> GetByDocumentAsync(Guid tenantId, string normalizedDocument, CancellationToken ct = default) =>
        await _context.Customers.FirstOrDefaultAsync(
            c => c.TenantId == tenantId && c.Document == normalizedDocument && c.IsActive, ct);

    public async Task<IReadOnlyList<Customer>> GetAllAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        // Join the tier in the same round-trip so the listing handler never queries per customer.
        var query = _context.Customers
            .Include(c => c.LoyaltyTier)
            .AsQueryable();
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query.ToListAsync(ct);
    }

    public async Task<bool> ExistsByDocumentAsync(string normalizedDocument, Guid? excludeId, CancellationToken ct = default) =>
        await _context.Customers.AnyAsync(
            c => c.Document == normalizedDocument && (excludeId == null || c.Id != excludeId), ct);

    public Task UpdateAsync(Customer customer, CancellationToken ct = default)
    {
        _context.Customers.Update(customer);
        return Task.CompletedTask;
    }
}
