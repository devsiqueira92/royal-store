using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using Serilog;
using Serilog.Enrichers.Sensitive;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace RoyalStore.API.Extensions;

internal static class SerilogExtensions
{
    internal static WebApplicationBuilder ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Serilog.Debugging.SelfLog.Enable(Console.Error);

        builder.Host.UseSerilog((context, loggerConfiguration) =>
        {
            var configuration = context.Configuration;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var retentionDays = configuration.GetValue("Logging:RetentionDays", 30);

            loggerConfiguration
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithSensitiveDataMasking(options =>
                {
                    options.MaskProperties.Add(new MaskProperty { Name = "Password" });
                    options.MaskProperties.Add(new MaskProperty { Name = "Token" });
                    options.MaskProperties.Add(new MaskProperty { Name = "Secret" });
                    options.MaskProperties.Add(new MaskProperty { Name = "SecretKey" });
                    options.MaskProperties.Add(new MaskProperty { Name = "AccessToken" });
                    options.MaskProperties.Add(new MaskProperty { Name = "RefreshToken" });
                })
                .WriteTo.Console();

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                loggerConfiguration.WriteTo.PostgreSQL(
                    connectionString: connectionString,
                    tableName: "Logs",
                    columnOptions: GetLogColumnOptions(),
                    needAutoCreateTable: true,
                    retentionTime: TimeSpan.FromDays(retentionDays));

                EnsureLogIndexes(connectionString);
            }
        });

        return builder;
    }

    // The sink creates the "Logs" table synchronously above (needAutoCreateTable),
    // but only manages columns, not query-pattern indexes — add them idempotently here.
    private static void EnsureLogIndexes(string connectionString)
    {
        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = """
                CREATE INDEX IF NOT EXISTS "IX_Logs_Timestamp" ON "Logs" ("Timestamp");
                CREATE INDEX IF NOT EXISTS "IX_Logs_TenantId" ON "Logs" ("TenantId");
                CREATE INDEX IF NOT EXISTS "IX_Logs_RequestId" ON "Logs" ("RequestId");
                CREATE INDEX IF NOT EXISTS "IX_Logs_Level" ON "Logs" ("Level");
                """;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Serilog.Debugging.SelfLog.WriteLine("Failed to ensure indexes on \"Logs\" table: {0}", ex);
        }
    }

    private static IDictionary<string, ColumnWriterBase> GetLogColumnOptions() =>
        new Dictionary<string, ColumnWriterBase>
        {
            { "Timestamp", new TimestampColumnWriter() },
            { "Level", new LevelColumnWriter(renderAsText: true) },
            { "Message", new RenderedMessageColumnWriter() },
            { "MessageTemplate", new MessageTemplateColumnWriter() },
            { "Exception", new ExceptionColumnWriter() },
            { "Properties", new PropertiesColumnWriter() },
            { "TenantId", new SinglePropertyColumnWriter("TenantId", PropertyWriteMethod.Raw, NpgsqlDbType.Uuid) },
            { "UserId", new SinglePropertyColumnWriter("UserId", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            { "RequestId", new SinglePropertyColumnWriter("RequestId", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            { "Endpoint", new SinglePropertyColumnWriter("Endpoint", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            { "HttpMethod", new SinglePropertyColumnWriter("RequestMethod", PropertyWriteMethod.Raw, NpgsqlDbType.Text) },
            { "StatusCode", new SinglePropertyColumnWriter("StatusCode", PropertyWriteMethod.Raw, NpgsqlDbType.Integer) },
            { "ElapsedMs", new SinglePropertyColumnWriter("Elapsed", PropertyWriteMethod.Raw, NpgsqlDbType.Double) },
        };
}
