using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RoyalStore.Domain.Expenses;
using RoyalStore.Infrastructure.Persistence;

namespace RoyalStore.Infrastructure.Seed;

/// <summary>
/// One-time backfill of the default operating-expense categories (FR-029) for tenants that existed
/// before this module shipped. New tenants get theirs inside RegisterTenantCommandHandler, in the
/// same transaction as the tenant itself.
/// </summary>
public static class ExpenseCategorySeeder
{
    public static async Task SeedDefaultsForExistingTenantsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<RoyalStoreContext>>();

        try
        {
            var context = services.GetRequiredService<RoyalStoreContext>();

            // Tenants carries no query filter, so this reads every store — which is the point.
            var tenantIds = await context.Tenants.Select(t => t.Id).ToListAsync();

            // IgnoreQueryFilters is correct and necessary here, and nowhere else in this module:
            // startup has no HTTP context and therefore no tenant, so the filter would match
            // nothing and this would re-seed every tenant on every boot. Every other read in the
            // module goes through the filter (Constitution V).
            var seededTenantIds = await context.ExpenseCategories
                .IgnoreQueryFilters()
                .Select(c => c.TenantId)
                .Distinct()
                .ToListAsync();

            var missing = tenantIds.Except(seededTenantIds).ToList();
            if (missing.Count == 0)
            {
                logger.LogInformation(
                    "Expense category backfill: nothing to do, {TenantCount} tenants already seeded.",
                    tenantIds.Count);
                return;
            }

            foreach (var tenantId in missing)
                await context.ExpenseCategories.AddRangeAsync(ExpenseCategory.SeedDefaults(tenantId));

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Expense category backfill: seeded defaults for {SeededTenantCount} tenants.",
                missing.Count);
        }
        catch (Exception ex)
        {
            // Never block startup over a backfill — the module still works for tenants that have
            // categories, and the failure is visible in the log.
            logger.LogError(ex, "Failed to backfill default expense categories.");
        }
    }
}
