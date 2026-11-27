using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Suppliers;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class SupplierRepository : ISupplierRepository
{
    private const string CnpjHash = nameof(Supplier.Cnpj) + "Hash";

    private readonly RoyalStoreContext _context;

    public SupplierRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Supplier supplier, CancellationToken ct = default) =>
        await _context.Suppliers.AddAsync(supplier, ct);

    public async Task<Supplier?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Supplier>> GetAllAsync(bool? isActive, CancellationToken ct = default)
    {
        var query = _context.Suppliers.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query.ToListAsync(ct);
    }

    // The document is encrypted with a random nonce, so it's matched on its blind index.
    public async Task<bool> ExistsByCnpjAsync(string cnpj, Guid? excludeId, CancellationToken ct = default)
    {
        var hash = _context.ComputeBlindIndex<Supplier>(nameof(Supplier.Cnpj), cnpj);
        return await _context.Suppliers.AnyAsync(
            s => EF.Property<string?>(s, CnpjHash) == hash && (excludeId == null || s.Id != excludeId),
            ct);
    }

    public Task UpdateAsync(Supplier supplier, CancellationToken ct = default)
    {
        _context.Suppliers.Update(supplier);
        return Task.CompletedTask;
    }
}
