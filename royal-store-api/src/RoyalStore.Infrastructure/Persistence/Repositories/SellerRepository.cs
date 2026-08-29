using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Sellers;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class SellerRepository : ISellerRepository
{
    private readonly RoyalStoreContext _context;

    public SellerRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Seller seller, CancellationToken ct = default) =>
        await _context.Sellers.AddAsync(seller, ct);

    public async Task<Seller?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Sellers.FirstOrDefaultAsync(s => s.Id == id, ct);

    // UserId is a globally unique FK to AspNetUsers.Id, so ignoring the tenant
    // query filter is safe here — it lets anonymous flows (login) and flows with
    // no tenant context resolve a seller by their account id.
    public async Task<Seller?> GetByUserIdAsync(string userId, CancellationToken ct = default) =>
        await _context.Sellers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task<IReadOnlyList<Seller>> GetAllAsync(bool? isActive, CancellationToken ct = default)
    {
        var query = _context.Sellers.AsQueryable();

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        return await query.ToListAsync(ct);
    }

    public async Task<bool> ExistsByCpfAsync(string cpf, Guid? excludeId, CancellationToken ct = default) =>
        await _context.Sellers.AnyAsync(
            s => s.Cpf == cpf && (excludeId == null || s.Id != excludeId),
            ct);

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId, CancellationToken ct = default) =>
        await _context.Sellers.AnyAsync(
            s => s.Email == email && (excludeId == null || s.Id != excludeId),
            ct);

    public Task UpdateAsync(Seller seller, CancellationToken ct = default)
    {
        _context.Sellers.Update(seller);
        return Task.CompletedTask;
    }
}
