using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Infrastructure.Persistence.Encryption;

namespace RoyalStore.Infrastructure.Tests.Persistence;

/// <summary>Shaped like the real mappings (private setters, nullable PII, a DateOnly, an owned
/// value object) so the shared encryption plumbing is exercised against a real relational store.</summary>
public sealed class Person
{
    private Person() { }

    public Person(Guid tenantId, string? document, string? phone, DateOnly? birthDate, Location location)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        Document = document;
        Phone = phone;
        BirthDate = birthDate;
        Location = location;
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string? Document { get; private set; }
    public string? Phone { get; private set; }
    public DateOnly? BirthDate { get; private set; }
    public Location Location { get; private set; } = new(null, null);

    public void Change(string? document, string? phone)
    {
        Document = document;
        Phone = phone;
    }
}

public sealed record Location(string? Street, string? City);

public sealed class EncryptionTestContext(DbContextOptions<EncryptionTestContext> options, IEncryptionService encryption)
    : DbContext(options), IEncryptedDbContext
{
    public const string DocumentHash = nameof(Person.Document) + "Hash";

    public IEncryptionService Encryption => encryption;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.ReplaceService<IModelCacheKeyFactory, EncryptionModelCacheKeyFactory>();

    public DbSet<Person> People => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(b =>
        {
            b.ToTable("People");
            b.HasKey(p => p.Id);
            b.Property(p => p.Document).IsEncrypted().HasMaxLength(14);
            b.HasBlindIndex(nameof(Person.Document), BlindIndexKind.Digits);
            b.Property(p => p.Phone).IsEncrypted();
            b.Property(p => p.BirthDate).IsEncrypted();
            b.HasIndex(nameof(Person.TenantId), DocumentHash).IsUnique();
            b.OwnsOne(p => p.Location, l =>
            {
                l.Property(x => x.Street).HasColumnName("Street").IsEncrypted();
                l.Property(x => x.City).HasColumnName("City");
            });
            b.Navigation(p => p.Location).IsRequired();
        });

        modelBuilder.ApplyEncryption(encryption);
    }

    /// <summary>A context on a fresh in-memory SQLite database that lives as long as
    /// <paramref name="connection"/> stays open.</summary>
    public static EncryptionTestContext Create(SqliteConnection connection, IEncryptionService encryption)
    {
        var options = new DbContextOptionsBuilder<EncryptionTestContext>()
            .UseSqlite(connection)
            .AddInterceptors(new BlindIndexInterceptor(encryption))
            .Options;

        return new EncryptionTestContext(options, encryption);
    }
}
