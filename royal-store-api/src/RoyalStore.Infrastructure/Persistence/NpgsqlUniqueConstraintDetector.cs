using Npgsql;
using RoyalStore.Application.Abstractions.Data;

namespace RoyalStore.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL reports a unique-constraint violation as SQLSTATE 23505. Matched on the state code
/// rather than the message text, so it survives a server locale change.
/// </summary>
public sealed class NpgsqlUniqueConstraintDetector : IUniqueConstraintDetector
{
    public bool IsUniqueViolation(Exception exception) => Find(exception) is not null;

    /// <remarks>For a unique index PostgreSQL reports the index name as the constraint name.</remarks>
    public bool IsUniqueViolation(Exception exception, string constraintName) =>
        string.Equals(Find(exception)?.ConstraintName, constraintName, StringComparison.Ordinal);

    private static PostgresException? Find(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } violation)
                return violation;
        }

        return null;
    }
}
