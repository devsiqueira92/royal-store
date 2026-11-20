using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Labels;

namespace RoyalStore.Infrastructure.Persistence.Repositories;

public sealed class LabelRepository : ILabelRepository
{
    private readonly RoyalStoreContext _context;

    public LabelRepository(RoyalStoreContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LabelTemplate label, CancellationToken ct = default) =>
        await _context.LabelTemplates.AddAsync(label, ct);

    public async Task<LabelTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.LabelTemplates
            .Include(t => t.Format)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<LabelTemplate?> GetDefaultAsync(CancellationToken ct = default) =>
        await _context.LabelTemplates
            .Include(t => t.Format)
            .FirstOrDefaultAsync(t => t.IsDefault, ct);

    public async Task<IReadOnlyList<LabelTemplate>> GetAllAsync(bool? isActive = null, CancellationToken ct = default)
    {
        var query = _context.LabelTemplates
            .AsNoTracking()
            .Include(t => t.Format)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        return await query
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNormalizedNameAsync(
        string normalizedName, Guid? excludeId, CancellationToken ct = default) =>
        await _context.LabelTemplates.AnyAsync(
            t => t.NormalizedName == normalizedName && (excludeId == null || t.Id != excludeId), ct);

    public void Remove(LabelTemplate label)
    {
        // The format belongs to this label alone (065 FR-021); the FK is Restrict, so both rows are
        // removed explicitly. EF deletes the template before the format it points at.
        _context.LabelTemplates.Remove(label);
        _context.LabelFormats.Remove(label.Format);
    }
}
