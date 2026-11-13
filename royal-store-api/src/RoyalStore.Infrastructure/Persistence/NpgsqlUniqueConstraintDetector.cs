using Npgsql;
using RoyalStore.Application.Abstractions.Data;

namespace RoyalStore.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL reports a unique-constraint violation as SQLSTATE 23505. Matched on the state code
/// rather than the message text, so it survives a server locale change.
/// </summary>
public sealed class NpgsqlUniqueConstraintDetector : IUniqueConstraintDetector
{
    public bool IsUniqueViolation(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                return true;
        }

        return false;
    }
}
