using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Fiscal;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class IbptTaxRepository : IIbptTaxRepository
{
    private readonly RoyalStoreContext _context;

    public IbptTaxRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task<IbptTax?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.IbptTaxes
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<IbptTax>> SearchAsync(string normalizedTerm, int take, CancellationToken ct = default)
    {
        var query = _context.IbptTaxes
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (normalizedTerm.Length > 0)
        {
            query = query.Where(x =>
                EF.Functions.Like(EF.Property<string>(x, "NormalizedCode"), normalizedTerm + "%") ||
                EF.Functions.Like(EF.Property<string>(x, "NormalizedText"), "%" + normalizedTerm + "%"));
        }

        return await query
            .OrderBy(x => x.Code)
            .ThenBy(x => x.EffectiveStart)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyDictionary<string, IbptTax>> GetCurrentByCodesAsync(
        IReadOnlyCollection<string> codes, DateOnly asOf, CancellationToken ct = default)
    {
        if (codes.Count == 0)
            return new Dictionary<string, IbptTax>();

        var rows = await _context.IbptTaxes
            .AsNoTracking()
            .Where(x => x.IsActive && codes.Contains(x.Code))
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.Code)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(x => x.EffectiveStart <= asOf && asOf <= x.EffectiveEnd)
                    .ThenByDescending(x => x.EffectiveStart)
                    .ThenByDescending(x => x.EffectiveEnd)
                    .ThenBy(x => x.Id)
                    .First());
    }
}
