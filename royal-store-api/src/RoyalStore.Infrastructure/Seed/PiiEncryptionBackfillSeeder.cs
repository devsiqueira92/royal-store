using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Infrastructure.Persistence;
using RoyalStore.Infrastructure.Persistence.Encryption;
using RoyalStore.Infrastructure.Security.Encryption;

namespace RoyalStore.Infrastructure.Seed;

/// <summary>
/// Brings every encrypted column to the current key: encrypts legacy plaintext rows (written before
/// the encryption migration), re-encrypts rows under a retired key version (rotation), and fills
/// missing blind indexes. Driven entirely by the EF model annotations, so a newly encrypted column
/// is covered without touching this class. Only rows needing work are read, so re-running is a
/// no-op.
/// <para>
/// Works on raw column values rather than through EF: the converters would refuse to read the
/// legacy plaintext this exists to fix.
/// </para>
/// </summary>
public static class PiiEncryptionBackfillSeeder
{
    private const int BatchSize = 500;

    public static async Task BackfillAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<RoyalStoreContext>>();
        var context = services.GetRequiredService<RoyalStoreContext>();
        var encryption = services.GetRequiredService<IEncryptionService>();
        var currentVersion = services.GetRequiredService<IOptions<EncryptionOptions>>().Value.KeyVersion;

        try
        {
            var total = await RunAsync(context, encryption, currentVersion, logger, ct);

            // Logged on every run, including the ones that do nothing, so silence is never
            // mistaken for success (Constitution VII).
            logger.LogInformation("PII encryption backfill: {Rows} rows updated (key version {Version}).",
                total, currentVersion);
        }
        catch (Exception ex)
        {
            // Unlike the other seeders this one fails startup: with plaintext or unknown-key rows
            // left behind, every read of them would fail anyway — better one loud error than a
            // half-working API. Fix the cause (usually a missing PreviousKeys entry) and restart.
            logger.LogCritical(ex, "PII encryption backfill failed.");
            throw;
        }
    }

    /// <returns>Rows updated across every encrypted table.</returns>
    internal static async Task<int> RunAsync(
        DbContext context, IEncryptionService encryption, int currentVersion, ILogger logger, CancellationToken ct)
    {
        var total = 0;
        foreach (var table in BuildPlans(context.Model))
        {
            var rows = await BackfillTableAsync(context, encryption, currentVersion, table, ct);
            if (rows > 0)
                logger.LogInformation("PII encryption backfill: {Rows} rows updated in {Table}.", rows, table.Table);
            total += rows;
        }

        return total;
    }

    internal sealed record BlindIndexColumn(string HashColumn, string SourceColumn, BlindIndexKind Kind);

    internal sealed record TablePlan(
        string Table,
        string? Schema,
        string KeyColumn,
        IReadOnlyList<string> EncryptedColumns,
        IReadOnlyList<BlindIndexColumn> BlindIndexes)
    {
        public string QualifiedName => Schema is null ? Quote(Table) : $"{Quote(Schema)}.{Quote(Table)}";
    }

    /// <summary>One plan per table, merging owned types that share their owner's table.</summary>
    internal static IReadOnlyList<TablePlan> BuildPlans(IModel model)
    {
        var plans = new Dictionary<(string Table, string? Schema), TablePlan>();

        foreach (var entityType in model.GetEntityTypes())
        {
            var table = entityType.GetTableName();
            if (table is null)
                continue;

            var schema = entityType.GetSchema();
            var store = StoreObjectIdentifier.Table(table, schema);

            var encrypted = entityType.GetProperties()
                .Where(p => p.FindAnnotation(EncryptionModelBuilderExtensions.EncryptedAnnotation)?.Value is true)
                .Select(p => p.GetColumnName(store)!)
                .ToList();

            var indexes = BlindIndex.Describe(entityType)
                .Select(i => new BlindIndexColumn(
                    entityType.FindProperty(i.HashProperty)!.GetColumnName(store)!,
                    entityType.FindProperty(i.SourceProperty)!.GetColumnName(store)!,
                    i.Kind))
                .ToList();

            if (encrypted.Count == 0 && indexes.Count == 0)
                continue;

            var key = entityType.FindPrimaryKey()?.Properties;
            if (key is not { Count: 1 })
                throw new InvalidOperationException(
                    $"{entityType.DisplayName()}: encrypted tables need a single-column primary key.");

            var keyColumn = key[0].GetColumnName(store)!;

            plans[(table, schema)] = plans.TryGetValue((table, schema), out var existing)
                ? existing with
                {
                    EncryptedColumns = [.. existing.EncryptedColumns.Union(encrypted)],
                    BlindIndexes = [.. existing.BlindIndexes.Union(indexes)],
                }
                : new TablePlan(table, schema, keyColumn, encrypted, indexes);
        }

        return [.. plans.Values];
    }

    private static async Task<int> BackfillTableAsync(
        DbContext context, IEncryptionService encryption, int currentVersion, TablePlan plan,
        CancellationToken ct)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await context.Database.OpenConnectionAsync(ct);

        var updated = 0;
        object? lastKey = null;

        while (true)
        {
            var batch = await ReadBatchAsync(connection, plan, currentVersion, lastKey, ct);
            if (batch.Count == 0)
                return updated;

            await using var transaction = await connection.BeginTransactionAsync(ct);
            foreach (var row in batch)
            {
                await UpdateRowAsync(connection, transaction, plan, Transform(row, plan, encryption, currentVersion), ct);
                lastKey = row[plan.KeyColumn];
            }

            await transaction.CommitAsync(ct);
            updated += batch.Count;
        }
    }

    /// <summary>Computes the new column values of one row from its raw values.</summary>
    internal static Dictionary<string, object?> Transform(
        IReadOnlyDictionary<string, object?> row, TablePlan plan, IEncryptionService encryption, int currentVersion)
    {
        var values = new Dictionary<string, object?> { [plan.KeyColumn] = row[plan.KeyColumn] };
        var plaintexts = new Dictionary<string, string?>();

        foreach (var column in plan.EncryptedColumns)
        {
            var raw = row[column] as string;
            if (raw is null)
            {
                plaintexts[column] = null;
                values[column] = null;
                continue;
            }

            var isCiphertext = EncryptedPayload.TryParse(raw, out var version, out _);
            var plain = isCiphertext ? encryption.Decrypt(raw) : raw;

            plaintexts[column] = plain;
            values[column] = isCiphertext && version == currentVersion ? raw : encryption.Encrypt(plain);
        }

        foreach (var index in plan.BlindIndexes)
        {
            var source = plaintexts.TryGetValue(index.SourceColumn, out var plain) ? plain : row[index.SourceColumn] as string;
            values[index.HashColumn] = source is null ? null : encryption.ComputeBlindIndex(source, index.Kind);
        }

        return values;
    }

    private static async Task<List<Dictionary<string, object?>>> ReadBatchAsync(
        DbConnection connection, TablePlan plan, int currentVersion, object? lastKey, CancellationToken ct)
    {
        var columns = plan.EncryptedColumns
            .Concat(plan.BlindIndexes.Select(i => i.SourceColumn))
            .Distinct()
            .Prepend(plan.KeyColumn)
            .ToList();

        var pending = plan.EncryptedColumns
            .Select(c => $"({Quote(c)} IS NOT NULL AND {Quote(c)} NOT LIKE @prefix)")
            .Concat(plan.BlindIndexes.Select(i => $"({Quote(i.SourceColumn)} IS NOT NULL AND {Quote(i.HashColumn)} IS NULL)"));

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT {string.Join(", ", columns.Select(Quote))}
            FROM {plan.QualifiedName}
            WHERE ({string.Join(" OR ", pending)}){(lastKey is null ? "" : $" AND {Quote(plan.KeyColumn)} > @lastKey")}
            ORDER BY {Quote(plan.KeyColumn)}
            LIMIT {BatchSize}
            """;
        AddParameter(command, "@prefix", EncryptedPayload.Prefix(currentVersion) + "%");
        if (lastKey is not null)
            AddParameter(command, "@lastKey", lastKey);

        var rows = new List<Dictionary<string, object?>>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var row = new Dictionary<string, object?>();
            for (var i = 0; i < columns.Count; i++)
                row[columns[i]] = await reader.IsDBNullAsync(i, ct) ? null : reader.GetValue(i);
            rows.Add(row);
        }

        return rows;
    }

    private static async Task UpdateRowAsync(
        DbConnection connection, DbTransaction transaction, TablePlan plan,
        IReadOnlyDictionary<string, object?> values, CancellationToken ct)
    {
        var assignments = values.Keys.Where(k => k != plan.KeyColumn).ToList();

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            $"UPDATE {plan.QualifiedName} SET {string.Join(", ", assignments.Select((c, i) => $"{Quote(c)} = @p{i}"))} " +
            $"WHERE {Quote(plan.KeyColumn)} = @key";

        for (var i = 0; i < assignments.Count; i++)
            AddParameter(command, $"@p{i}", values[assignments[i]]);
        AddParameter(command, "@key", values[plan.KeyColumn]);

        await command.ExecuteNonQueryAsync(ct);
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        if (value is null)
            parameter.DbType = System.Data.DbType.String;
        command.Parameters.Add(parameter);
    }

    private static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
}
