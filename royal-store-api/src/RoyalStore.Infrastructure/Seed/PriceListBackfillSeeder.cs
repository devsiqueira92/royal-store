using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoyalStore.Domain.Catalog;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Seed;

/// <summary>
/// Gives every tenant without a default price list one (061 FR-007). Only missing rows are created,
/// so re-running changes nothing. New tenants get their list inside RegisterTenantCommandHandler.
/// <para>
/// Products that predate price lists got their default-list price from the former single sale price
/// in the <c>RemoveLegacySalePrices</c> migration, which also dropped that column.
/// </para>
/// </summary>
public static class PriceListBackfillSeeder
{
    public static async Task BackfillAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<RoyalStoreContext>>();

        try
        {
            var context = services.GetRequiredService<RoyalStoreContext>();

            var tenantsSeeded = await SeedDefaultListsAsync(context);

            // Logged on every run, including the ones that do nothing, so silence is never
            // mistaken for success (Constitution VII).
            logger.LogInformation(
                "Price list backfill: {TenantsSeeded} tenants received a default list.", tenantsSeeded);
        }
        catch (Exception ex)
        {
            // Never block startup over a backfill — the failure is visible in the log and the next
            // start picks up where this one stopped.
            logger.LogError(ex, "Failed to backfill price lists.");
        }
    }

    private static async Task<int> SeedDefaultListsAsync(RoyalStoreContext context)
    {
        // Tenants carries no query filter, so this reads every store — which is the point.
        var tenantIds = await context.Tenants.Select(t => t.Id).ToListAsync();

        // IgnoreQueryFilters is correct and necessary here: startup has no HTTP context and
        // therefore no tenant, so the filter would match nothing and every tenant would be
        // re-seeded on every boot. Every request-time read goes through the filter (Constitution V).
        var tenantsWithDefault = await context.PriceLists
            .IgnoreQueryFilters()
            .Where(l => l.IsDefault)
            .Select(l => l.TenantId)
            .ToListAsync();

        var missing = tenantIds.Except(tenantsWithDefault).ToList();
        if (missing.Count == 0)
            return 0;

        foreach (var tenantId in missing)
            await context.PriceLists.AddRangeAsync(PriceList.SeedDefaults(tenantId));

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return missing.Count;
    }
}
