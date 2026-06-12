using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Security;
using RoyalStore.Domain.Tenants;

namespace RoyalStore.Infrastructure.Persistence;

public sealed class RoyalStoreContext : IdentityDbContext<UserEntity>, IUnitOfWork
{
    private readonly ITenantContext _tenantContext;

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; } = null!;
    public DbSet<SecurityAuditEvent> SecurityAuditEvents { get; set; } = null!;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssemblyReference).Assembly);
    }
}
