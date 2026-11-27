using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Profiles;
using RoyalStore.Domain.Sales;
using RoyalStore.Domain.Suppliers;
using RoyalStore.Domain.Tenants;
using RoyalStore.Infrastructure.Persistence;
using RoyalStore.Infrastructure.Seed;
using RoyalStore.Infrastructure.Tests.Security;

namespace RoyalStore.Infrastructure.Tests.Persistence;

/// <summary>Pins the production mapping: which columns are encrypted and which carry a blind
/// index. Builds the real Npgsql model without touching a database.</summary>
public sealed class RoyalStoreModelEncryptionTests
{
    private static readonly IModel Model = CreateContext().Model;

    private static RoyalStoreContext CreateContext() => new(
        new DbContextOptionsBuilder<RoyalStoreContext>().UseNpgsql("Host=unused").Options,
        new FixedTenant(),
        TestKeys.Service());

    private sealed class FixedTenant : ITenantContext
    {
        public Guid TenantId => Guid.Empty;
    }

    public static TheoryData<Type, string> EncryptedProperties => new()
    {
        { typeof(Customer), nameof(Customer.Document) },
        { typeof(Customer), nameof(Customer.Phone) },
        { typeof(Customer), nameof(Customer.BirthDate) },
        { typeof(CustomerAddress), nameof(CustomerAddress.Street) },
        { typeof(CustomerAddress), nameof(CustomerAddress.Number) },
        { typeof(CustomerAddress), nameof(CustomerAddress.Complement) },
        { typeof(CustomerAddress), nameof(CustomerAddress.AdditionalInfo) },
        { typeof(Profile), nameof(Profile.Cpf) },
        { typeof(Profile), nameof(Profile.Email) },
        { typeof(Profile), nameof(Profile.Phone) },
        { typeof(Supplier), nameof(Supplier.Cnpj) },
        { typeof(Supplier), nameof(Supplier.Phone) },
        { typeof(Supplier), nameof(Supplier.Address) },
        { typeof(Tenant), nameof(Tenant.Document) },
        { typeof(SaleAddress), nameof(SaleAddress.Street) },
        { typeof(SaleAddress), nameof(SaleAddress.Number) },
    };

    [Theory]
    [MemberData(nameof(EncryptedProperties))]
    public void Pii_columns_are_encrypted_text(Type entity, string property)
    {
        var mapped = Model.FindEntityType(entity)!.FindProperty(property)!;

        Assert.NotNull(mapped.GetValueConverter());
        Assert.Equal("text", mapped.GetColumnType());
    }

    // Mirrors the hash names the repositories query by.
    public static TheoryData<Type, string, string[]> BlindIndexes => new()
    {
        { typeof(Customer), "DocumentHash", [nameof(Customer.TenantId), "DocumentHash"] },
        { typeof(Profile), "CpfHash", [nameof(Profile.TenantId), "CpfHash"] },
        { typeof(Profile), "EmailHash", [nameof(Profile.TenantId), "EmailHash"] },
        { typeof(Supplier), "CnpjHash", [nameof(Supplier.TenantId), "CnpjHash"] },
        { typeof(Tenant), "DocumentHash", ["DocumentHash"] },
    };

    [Theory]
    [MemberData(nameof(BlindIndexes))]
    public void Lookup_fields_have_a_unique_blind_index(Type entity, string hashProperty, string[] indexColumns)
    {
        var entityType = Model.FindEntityType(entity)!;
        var hash = entityType.FindProperty(hashProperty);

        Assert.NotNull(hash);
        Assert.True(hash.IsShadowProperty());
        Assert.Null(hash.GetValueConverter());
        Assert.Contains(entityType.GetIndexes(),
            i => i.IsUnique && i.Properties.Select(p => p.Name).SequenceEqual(indexColumns));
    }

    [Fact]
    public void Ciphertext_columns_are_never_indexed()
    {
        var offending = Model.GetEntityTypes()
            .SelectMany(e => e.GetIndexes())
            .SelectMany(i => i.Properties)
            .Where(p => p.GetValueConverter() is not null && p.GetColumnType() == "text" &&
                        p.FindAnnotation("RoyalStore:Encrypted") is not null)
            .Select(p => $"{p.DeclaringType.DisplayName()}.{p.Name}")
            .ToList();

        Assert.Empty(offending);
    }

    [Fact]
    public void Backfill_covers_every_encrypted_table()
    {
        var tables = PiiEncryptionBackfillSeeder.BuildPlans(Model).ToDictionary(p => p.Table);

        Assert.Equal(["CustomerAddresses", "Customers", "Profiles", "Sales", "Suppliers", "Tenants"],
            tables.Keys.Order());
        Assert.Equal(["AddressNumber", "AddressStreet"], tables["Sales"].EncryptedColumns.Order());
        Assert.Equal("Id", tables["Sales"].KeyColumn);
        Assert.Equal(2, tables["Profiles"].BlindIndexes.Count);
    }

    [Fact]
    public void Context_blind_index_uses_the_mapped_normalization()
    {
        using var context = CreateContext();

        Assert.Equal(
            context.ComputeBlindIndex<Customer>(nameof(Customer.Document), "12345678909"),
            context.ComputeBlindIndex<Customer>(nameof(Customer.Document), "123.456.789-09"));
        Assert.Equal(
            context.ComputeBlindIndex<Profile>(nameof(Profile.Email), "a@b.com"),
            context.ComputeBlindIndex<Profile>(nameof(Profile.Email), " A@B.com "));
        Assert.Throws<InvalidOperationException>(() =>
            context.ComputeBlindIndex<Customer>(nameof(Customer.Phone), "1"));
    }
}
