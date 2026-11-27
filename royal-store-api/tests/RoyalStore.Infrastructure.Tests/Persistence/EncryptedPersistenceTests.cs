using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Infrastructure.Security.Encryption;
using RoyalStore.Infrastructure.Tests.Security;

namespace RoyalStore.Infrastructure.Tests.Persistence;

public sealed class EncryptedPersistenceTests : IDisposable
{
    private static readonly Guid Tenant = Guid.NewGuid();

    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    private readonly AesGcmEncryptionService _encryption = TestKeys.Service();

    public EncryptedPersistenceTests()
    {
        _connection.Open();
        using var context = NewContext();
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private EncryptionTestContext NewContext() => EncryptionTestContext.Create(_connection, _encryption);

    private async Task<Person> SaveAsync(Person person)
    {
        await using var context = NewContext();
        context.People.Add(person);
        await context.SaveChangesAsync();
        return person;
    }

    private async Task<Dictionary<string, object?>> RawRowAsync(Guid id)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = """SELECT "Document", "DocumentHash", "Phone", "BirthDate", "Street", "City" FROM "People" WHERE "Id" = $id""";
        command.Parameters.AddWithValue("$id", id.ToString().ToUpperInvariant());
        await using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        return Enumerable.Range(0, reader.FieldCount)
            .ToDictionary(reader.GetName, i => reader.IsDBNull(i) ? null : reader.GetValue(i));
    }

    [Fact]
    public async Task Stores_ciphertext_and_blind_index_but_reads_back_plaintext()
    {
        var person = await SaveAsync(new Person(Tenant, "12345678909", "41999990000",
            new DateOnly(1990, 5, 1), new Location("Rua XV de Novembro", "Curitiba")));

        var raw = await RawRowAsync(person.Id);
        foreach (var column in new[] { "Document", "Phone", "BirthDate", "Street" })
        {
            var stored = Assert.IsType<string>(raw[column]);
            Assert.StartsWith("v1:", stored);
        }
        Assert.Equal("Curitiba", raw["City"]);
        Assert.Equal(_encryption.ComputeBlindIndex("12345678909", BlindIndexKind.Digits), raw["DocumentHash"]);

        await using var context = NewContext();
        var loaded = await context.People.SingleAsync(p => p.Id == person.Id);
        Assert.Equal("12345678909", loaded.Document);
        Assert.Equal("41999990000", loaded.Phone);
        Assert.Equal(new DateOnly(1990, 5, 1), loaded.BirthDate);
        Assert.Equal("Rua XV de Novembro", loaded.Location.Street);
    }

    [Fact]
    public async Task Null_values_stay_null_with_no_blind_index()
    {
        var person = await SaveAsync(new Person(Tenant, null, null, null, new Location(null, null)));

        var raw = await RawRowAsync(person.Id);
        Assert.All(raw.Values, Assert.Null);

        await using var context = NewContext();
        var loaded = await context.People.SingleAsync(p => p.Id == person.Id);
        Assert.Null(loaded.Document);
        Assert.Null(loaded.BirthDate);
    }

    [Fact]
    public async Task Finds_rows_by_blind_index_regardless_of_formatting()
    {
        var person = await SaveAsync(new Person(Tenant, "12345678909", null, null, new Location(null, null)));
        await SaveAsync(new Person(Tenant, "98765432100", null, null, new Location(null, null)));

        var hash = _encryption.ComputeBlindIndex("123.456.789-09", BlindIndexKind.Digits);
        await using var context = NewContext();
        var found = await context.People
            .Where(p => EF.Property<string?>(p, EncryptionTestContext.DocumentHash) == hash)
            .ToListAsync();

        Assert.Equal(person.Id, Assert.Single(found).Id);
    }

    [Fact]
    public async Task Updating_the_source_value_refreshes_the_blind_index()
    {
        var person = await SaveAsync(new Person(Tenant, "12345678909", null, null, new Location(null, null)));

        await using (var context = NewContext())
        {
            var tracked = await context.People.SingleAsync(p => p.Id == person.Id);
            tracked.Change("98765432100", "41999990000");
            await context.SaveChangesAsync();
        }

        var raw = await RawRowAsync(person.Id);
        Assert.Equal(_encryption.ComputeBlindIndex("98765432100", BlindIndexKind.Digits), raw["DocumentHash"]);

        await using (var context = NewContext())
        {
            var tracked = await context.People.SingleAsync(p => p.Id == person.Id);
            tracked.Change(null, null);
            await context.SaveChangesAsync();
        }

        Assert.Null((await RawRowAsync(person.Id))["DocumentHash"]);
    }

    [Fact]
    public async Task Unique_index_on_the_blind_index_rejects_duplicates_within_a_tenant()
    {
        await SaveAsync(new Person(Tenant, "12345678909", null, null, new Location(null, null)));
        await SaveAsync(new Person(Guid.NewGuid(), "12345678909", null, null, new Location(null, null)));

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            SaveAsync(new Person(Tenant, "123.456.789-09", null, null, new Location(null, null))));
    }

    [Fact]
    public void Encrypted_columns_ignore_configured_max_length()
    {
        using var context = NewContext();
        var property = context.Model.FindEntityType(typeof(Person))!.FindProperty(nameof(Person.Document))!;

        Assert.Null(property.GetMaxLength());
        Assert.NotNull(property.GetValueConverter());
    }
}
