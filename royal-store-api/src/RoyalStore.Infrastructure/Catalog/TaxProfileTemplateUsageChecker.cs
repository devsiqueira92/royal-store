using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Catalog;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Catalog;

public sealed class TaxProfileTemplateUsageChecker : ITaxProfileTemplateUsageChecker
{
    private readonly RoyalStoreContext _context;

    public TaxProfileTemplateUsageChecker(RoyalStoreContext context)
    {
        _context = context;
    }

    public Task<bool> IsInUseAsync(Guid templateId, CancellationToken ct = default) =>
        _context.Products.AnyAsync(p => p.TaxProfileTemplateId == templateId, ct);
}
