using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;

namespace RoyalStore.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<RoyalStoreContext>
{
    public RoyalStoreContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RoyalStoreContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=royalstore_db;Username=postgres;Password=sua_senha_local_aqui;");

        return new RoyalStoreContext(optionsBuilder.Options, new NullTenantContext(), new DesignTimeEncryptionService());
    }

    private sealed class NullTenantContext : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
    }

    /// <summary>Migrations only need the model's shape (encrypted columns map to text); nothing is
    /// ever encrypted at design time.</summary>
    private sealed class DesignTimeEncryptionService : IEncryptionService
    {
        public string Encrypt(string plainText) => throw NotAvailable();
        public string Decrypt(string cipherText) => throw NotAvailable();
        public string? EncryptNullable(string? plainText) => throw NotAvailable();
        public string? DecryptNullable(string? cipherText) => throw NotAvailable();
        public string ComputeBlindIndex(string plainText) => throw NotAvailable();
        public string ComputeBlindIndex(string plainText, BlindIndexKind kind) => throw NotAvailable();

        private static NotSupportedException NotAvailable() =>
            new("Encryption is not available at design time.");
    }
}
