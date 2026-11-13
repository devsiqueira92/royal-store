using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;
using RoyalStore.Domain.Fiscal;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Loyalty;
using RoyalStore.Domain.Payments;
using RoyalStore.Domain.Permissions;
using RoyalStore.Domain.Profiles;
using RoyalStore.Domain.Security;
using RoyalStore.Domain.Sales;
using RoyalStore.Domain.Sellers;
using RoyalStore.Domain.Suppliers;
using RoyalStore.Domain.Tenants;

namespace RoyalStore.Infrastructure.Persistence;

public sealed class RoyalStoreContext : IdentityDbContext<UserEntity>, IUnitOfWork
{
    private readonly ITenantContext _tenantContext;

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
    public DbSet<ProductPhoto> ProductPhotos { get; set; } = null!;
    public DbSet<TaxProfileTemplate> TaxProfileTemplates { get; set; } = null!;
    public DbSet<CashShift> CashShifts { get; set; } = null!;
    public DbSet<CashMovement> CashMovements { get; set; } = null!;
    public DbSet<Sale> Sales { get; set; } = null!;
    public DbSet<SaleItem> SaleItems { get; set; } = null!;
    public DbSet<SalePayment> SalePayments { get; set; } = null!;
    public DbSet<Seller> Sellers { get; set; } = null!;
    public DbSet<Profile> Profiles { get; set; } = null!;
    public DbSet<Supplier> Suppliers { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<CustomerAddress> CustomerAddresses { get; set; } = null!;
    public DbSet<LoyaltyTier> LoyaltyTiers { get; set; } = null!;
    public DbSet<CashbackEntry> CashbackEntries { get; set; } = null!;
    public DbSet<CashbackRedemption> CashbackRedemptions { get; set; } = null!;
    public DbSet<Clawback> Clawbacks { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;

    // Operating expenses (OPEX). Deliberately unrelated to Sales/Inventory: this module records
    // what it costs to keep the doors open, never merchandise for resale or fixed assets (FR-011).
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<ExpensePayment> ExpensePayments { get; set; } = null!;
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; } = null!;
    public DbSet<ExpenseRecurrence> ExpenseRecurrences { get; set; } = null!;
    public DbSet<ExpenseAttachment> ExpenseAttachments { get; set; } = null!;
    public DbSet<CostCenter> CostCenters { get; set; } = null!;
    public DbSet<FinancialAccount> FinancialAccounts { get; set; } = null!;
    public DbSet<UserPermissionGrant> UserPermissionGrants { get; set; } = null!;
    public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; } = null!;
    public DbSet<SecurityAuditEvent> SecurityAuditEvents { get; set; } = null!;
    public DbSet<ConfirmationResendRequest> ConfirmationResendRequests { get; set; } = null!;
    public DbSet<EmailChangeRequest> EmailChangeRequests { get; set; } = null!;

    // Global reference data — shared across all tenants, no TenantId, no query filter.
    public DbSet<IbptTax> IbptTaxes { get; set; } = null!;
    public DbSet<OriginOption> OriginOptions { get; set; } = null!;

    public DbSet<UnitMeasureOption> UnitMeasureOptions { get; set; } = null!;

    public RoyalStoreContext(DbContextOptions<RoyalStoreContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        base.SaveChangesAsync(cancellationToken);

    public async Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        var result = await operation(cancellationToken);
        if (result.IsSuccess)
            await transaction.CommitAsync(cancellationToken);
        else
            await transaction.RollbackAsync(cancellationToken);
        return result;
    }

    /// <summary>Maps the Postgres function created by the 052-product-filters migration so it can be
    /// called from LINQ (translated to SQL) without a new migration or computed column (057 research
    /// Decision 3). Never call this directly — it only exists for EF's expression translation.</summary>
    [DbFunction("fn_normalize_search_text")]
    public static string NormalizeSearchText(string? input) =>
        throw new NotSupportedException("Only for use in LINQ expressions translated to SQL.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);

        modelBuilder.HasDbFunction(typeof(RoyalStoreContext).GetMethod(nameof(NormalizeSearchText))!);

        modelBuilder.Entity<Category>()
            .HasQueryFilter(c => c.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => p.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ProductVariant>()
            .HasQueryFilter(v => v.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ProductPhoto>()
            .HasQueryFilter(p => p.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<TaxProfileTemplate>()
            .HasQueryFilter(t => t.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Seller>()
            .HasQueryFilter(s => s.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Profile>()
            .HasQueryFilter(p => p.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Supplier>()
            .HasQueryFilter(s => s.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<UserPermissionGrant>()
            .HasQueryFilter(g => g.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<CashShift>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Sale>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<SaleItem>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<SalePayment>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Customer>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<CustomerAddress>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<LoyaltyTier>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<CashbackEntry>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<CashbackRedemption>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Clawback>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Payment>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<CashMovement>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<UnitMeasureOption>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Expense>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ExpensePayment>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ExpenseCategory>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ExpenseRecurrence>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<ExpenseAttachment>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<CostCenter>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<FinancialAccount>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        // OriginOption is deliberately global — no query filter, same as IbptTax.
    }
}
