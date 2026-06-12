using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RoyalStore.Application.Abstractions.Services;

namespace RoyalStore.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RoyalStoreContext>
{
    public RoyalStoreContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RoyalStoreContext>();
        optionsBuilder.UseSqlServer(
            "Server=localhost\\SQLEXPRESS;Database=RoyalStore;Trusted_Connection=True;TrustServerCertificate=True;");

        return new RoyalStoreContext(optionsBuilder.Options, new NullTenantContext());
    }

    private sealed class NullTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
    }
}
