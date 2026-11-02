using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RoyalStore.Application.Abstractions.Services;

namespace RoyalStore.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RoyalStoreContext>
{
    public RoyalStoreContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RoyalStoreContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=royalstore_db;Username=postgres;Password=sua_senha_local_aqui;");

        return new RoyalStoreContext(optionsBuilder.Options, new NullTenantContext());
    }

    private sealed class NullTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
    }
}
