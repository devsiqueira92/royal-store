using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.DTOs;
using RoyalStore.Application.Labels.DTOs;
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

    public async Task<IReadOnlyList<LabelSummaryResponse>> GetSummariesAsync(
        bool? isActive = null, CancellationToken ct = default)
    {
        // System labels are never listed here.
        var query = _context.LabelTemplates
            .AsNoTracking()
            .Where(t => !t.Format.IsSystem);

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        // Scalar columns only — the layout document is never read for the listing.
        return await query
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .Select(t => new LabelSummaryResponse(
                t.Id,
                t.Name,
                t.Format.PaperType,
                t.Format.LabelWidth,
                t.Format.LabelHeight,
                t.Format.Columns,
                t.IsActive,
                // A change to the measurements alone updates only the format row (065 research D11).
                t.Format.UpdatedAt > t.UpdatedAt ? t.Format.UpdatedAt : t.UpdatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OptionResponse>> GetActiveOptionsAsync(CancellationToken ct = default) =>
        await _context.LabelTemplates
            .AsNoTracking()
            .Where(t => t.IsActive && t.TenantId != Guid.Empty) // the store's own only (066 FR-008)
            .OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .Select(t => new OptionResponse(t.Id.ToString(), t.Name))
            .ToListAsync(ct);

    public async Task<bool> ExistsByNormalizedNameAsync(
        string normalizedName, Guid? excludeId, CancellationToken ct = default) =>
        await _context.LabelTemplates.AnyAsync(
            // A system configuration's name does not block a store label of the same name (066 FR-007).
            t => t.TenantId != Guid.Empty
                 && t.NormalizedName == normalizedName && (excludeId == null || t.Id != excludeId), ct);

    public async Task<IReadOnlyList<OptionResponse>> GetConfigurationOptionsAsync(CancellationToken ct = default) =>
        await _context.LabelTemplates
            .AsNoTracking()
            // The query filter already limits rows to this store and the system (066 FR-001, FR-002).
            .Where(t => t.TenantId == Guid.Empty || t.IsActive)
            .OrderByDescending(t => t.TenantId == Guid.Empty)
            .ThenBy(t => t.Name)
            .Select(t => new OptionResponse(t.Id.ToString(), t.Name))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<OptionResponse>> GetPrintOptionsAsync(CancellationToken ct = default) =>
        await _context.LabelTemplates
            .AsNoTracking()
            // The query filter already limits rows to this store and the system.
            .Where(t => t.TenantId == Guid.Empty || t.IsActive)
            .OrderBy(t => t.TenantId == Guid.Empty)
            .ThenByDescending(t => t.IsDefault)
            .ThenBy(t => t.Name)
            .Select(t => new OptionResponse(t.Id.ToString(), t.Name))
            .ToListAsync(ct);

    public void Remove(LabelTemplate label)
    {
        // The format belongs to this label alone (065 FR-021); the FK is Restrict, so both rows are
        // removed explicitly. EF deletes the template before the format it points at.
        _context.LabelTemplates.Remove(label);
        _context.LabelFormats.Remove(label.Format);
    }
}
