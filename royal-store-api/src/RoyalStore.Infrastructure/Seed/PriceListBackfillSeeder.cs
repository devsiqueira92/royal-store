using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoyalStore.Domain.Catalog;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Seed;

/// <summary>
/// Carries stores that existed before price lists (061) over to the new structure:
/// every tenant without a default price list gets one (FR-007), and every product without a price
/// on its tenant's default list gets one equal to its former single sale price (FR-021).
/// Only missing rows are created, so re-running changes nothing and never overwrites a price
/// maintained through the new structure (FR-022). New tenants get their list inside
/// RegisterTenantCommandHandler; new products get their prices through Product.SetPrices.
/// </summary>
public static class PriceListBackfillSeeder
{
    private const int BatchSize = 500;

    public static async Task BackfillAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<RoyalStoreContext>>();

        try
        {
            var context = services.GetRequiredService<RoyalStoreContext>();

            var tenantsSeeded = await SeedDefaultListsAsync(context);
            var pricesCreated = await BackfillProductPricesAsync(context, logger);

            // Logged on every run, including the ones that do nothing, so silence is never
            // mistaken for success (Constitution VII).
            logger.LogInformation(
                "Price list backfill: {TenantsSeeded} tenants received a default list, {PricesCreated} product prices created.",
                tenantsSeeded, pricesCreated);
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

        // IgnoreQueryFilters is correct and necessary here and in the product query below, and
        // nowhere else in this feature: startup has no HTTP context and therefore no tenant, so the
        // filter would match nothing and every tenant would be re-seeded on every boot. Every
        // request-time read goes through the filter (Constitution V).
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

    private static async Task<int> BackfillProductPricesAsync(RoyalStoreContext context, ILogger logger)
    {
        var defaultLists = await context.PriceLists
            .IgnoreQueryFilters()
            .Where(l => l.IsDefault)
            .Select(l => new { l.TenantId, l.Id })
            .ToListAsync();

        var created = 0;

        foreach (var defaultList in defaultLists)
        {
            while (true)
            {
                // Services keep their single price and are not migrated (FR-029). Inactive products
                // are included: they can be re-enabled and must have a default-list price by then.
                var batch = await context.Products
                    .IgnoreQueryFilters()
                    .Where(p => p.TenantId == defaultList.TenantId
                        && p.Type == ItemType.Product
                        && !p.Prices.Any(pp => pp.PriceListId == defaultList.Id))
                    .OrderBy(p => p.Id)
                    .Select(p => new { p.Id, p.BasePrice })
                    .Take(BatchSize)
                    .ToListAsync();

                if (batch.Count == 0)
                    break;

                var createdInBatch = 0;
                foreach (var product in batch)
                {
                    var price = ProductPrice.CreateFromLegacyPrice(
                        defaultList.TenantId, product.Id, defaultList.Id, product.BasePrice);

                    if (price.IsFailure)
                    {
                        logger.LogWarning(
                            "Price list backfill: skipped product {ProductId} of tenant {TenantId}: {ErrorCode}.",
                            product.Id, defaultList.TenantId, price.Error.Code);
                        continue;
                    }

                    await context.ProductPrices.AddAsync(price.Value);
                    createdInBatch++;
                }

                // A batch that produced nothing would be selected again unchanged — stop instead
                // of looping forever on rows that cannot be carried over.
                if (createdInBatch == 0)
                    break;

                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
                created += createdInBatch;
            }
        }

        return created;
    }
}
