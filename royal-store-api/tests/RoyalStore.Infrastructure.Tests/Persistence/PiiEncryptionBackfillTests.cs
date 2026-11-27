using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Infrastructure.Security.Encryption;
using RoyalStore.Infrastructure.Seed;
using RoyalStore.Infrastructure.Tests.Security;

namespace RoyalStore.Infrastructure.Tests.Persistence;

public sealed class PiiEncryptionBackfillTests : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public PiiEncryptionBackfillTests()
    {
        _connection.Open();
        using var context = EncryptionTestContext.Create(_connection, TestKeys.Service());
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    private async Task<Guid> InsertRawAsync(string? document, string? phone, string? birthDate, string? street)
    {
        var id = Guid.NewGuid();
        await using var command = _connection.CreateCommand();
        command.CommandText = """
            INSERT INTO "People" ("Id", "TenantId", "Document", "Phone", "BirthDate", "Street", "City")
            VALUES ($id, $tenant, $document, $phone, $birthDate, $street, 'Curitiba')
            """;
        command.Parameters.AddWithValue("$id", id.ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$tenant", Guid.NewGuid().ToString().ToUpperInvariant());
        command.Parameters.AddWithValue("$document", (object?)document ?? DBNull.Value);
        command.Parameters.AddWithValue("$phone", (object?)phone ?? DBNull.Value);
        command.Parameters.AddWithValue("$birthDate", (object?)birthDate ?? DBNull.Value);
        command.Parameters.AddWithValue("$street", (object?)street ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
        return id;
    }

    private async Task<string?> RawAsync(Guid id, string column)
    {
        await using var command = _connection.CreateCommand();
        command.CommandText = $"""SELECT "{column}" FROM "People" WHERE "Id" = $id""";
        command.Parameters.AddWithValue("$id", id.ToString().ToUpperInvariant());
        return await command.ExecuteScalarAsync() as string;
    }

    private static async Task<int> RunAsync(EncryptionTestContext context, AesGcmEncryptionService encryption) =>
        await PiiEncryptionBackfillSeeder.RunAsync(context, encryption, encryption.CurrentKeyVersion,
            NullLogger.Instance, CancellationToken.None);

    [Fact]
    public async Task Encrypts_legacy_plaintext_and_fills_blind_indexes()
    {
        var encryption = TestKeys.Service();
        var legacy = await InsertRawAsync("12345678909", "41999990000", "1990-05-01", "Rua XV");
        var empty = await InsertRawAsync(null, null, null, null);

        await using var context = EncryptionTestContext.Create(_connection, encryption);
        Assert.Equal(1, await RunAsync(context, encryption));

        Assert.StartsWith("v1:", await RawAsync(legacy, "Document"));
        Assert.StartsWith("v1:", await RawAsync(legacy, "BirthDate"));
        Assert.Equal("Curitiba", await RawAsync(legacy, "City"));
        Assert.Equal(encryption.ComputeBlindIndex("12345678909", BlindIndexKind.Digits),
            await RawAsync(legacy, "DocumentHash"));
        Assert.Null(await RawAsync(empty, "DocumentHash"));

        var loaded = await context.People.AsNoTracking().SingleAsync(p => p.Id == legacy);
        Assert.Equal("12345678909", loaded.Document);
        Assert.Equal(new DateOnly(1990, 5, 1), loaded.BirthDate);
        Assert.Equal("Rua XV", loaded.Location.Street);
    }

    [Fact]
    public async Task Is_idempotent()
    {
        var encryption = TestKeys.Service();
        var id = await InsertRawAsync("12345678909", null, null, null);

        await using var context = EncryptionTestContext.Create(_connection, encryption);
        await RunAsync(context, encryption);
        var first = await RawAsync(id, "Document");

        Assert.Equal(0, await RunAsync(context, encryption));
        Assert.Equal(first, await RawAsync(id, "Document"));
    }

    [Fact]
    public async Task Re_encrypts_rows_under_a_retired_key_version()
    {
        var v1 = TestKeys.Service();
        var id = await InsertRawAsync(v1.Encrypt("12345678909"), v1.Encrypt("41999990000"), null, null);

        var v2 = TestKeys.RotatedService();
        await using var context = EncryptionTestContext.Create(_connection, v2);
        Assert.Equal(1, await RunAsync(context, v2));

        Assert.StartsWith("v2:", await RawAsync(id, "Document"));
        Assert.StartsWith("v2:", await RawAsync(id, "Phone"));
        Assert.Equal("12345678909", (await context.People.AsNoTracking().SingleAsync(p => p.Id == id)).Document);
    }

    [Fact]
    public async Task Processes_more_rows_than_one_batch()
    {
        var encryption = TestKeys.Service();
        for (var i = 0; i < 1_100; i++)
            await InsertRawAsync($"{i:00000000000}", null, null, null);

        await using var context = EncryptionTestContext.Create(_connection, encryption);
        Assert.Equal(1_100, await RunAsync(context, encryption));
        Assert.Equal(0, await RunAsync(context, encryption));
    }
}
