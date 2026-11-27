using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using RoyalStore.Application.Abstractions.Security;

namespace RoyalStore.Infrastructure.Persistence.Encryption;

/// <summary>
/// Recomputes every blind index (<c>…Hash</c> shadow column) of added/modified entities from the
/// in-memory plaintext right before saving, so the domain never has to know the hashes exist.
/// </summary>
public sealed class BlindIndexInterceptor : SaveChangesInterceptor
{
    private readonly IEncryptionService _encryption;
    private readonly ConcurrentDictionary<IEntityType, IReadOnlyList<BlindIndexDescriptor>> _cache = new();

    public BlindIndexInterceptor(IEncryptionService encryption)
    {
        _encryption = encryption;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            Apply(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            Apply(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified))
                continue;

            var indexes = _cache.GetOrAdd(entry.Metadata, BlindIndex.Describe);
            foreach (var index in indexes)
            {
                var source = entry.Property(index.SourceProperty).CurrentValue as string;
                var hash = source is null ? null : _encryption.ComputeBlindIndex(source, index.Kind);

                var hashProperty = entry.Property(index.HashProperty);
                if (!string.Equals(hashProperty.CurrentValue as string, hash, StringComparison.Ordinal))
                    hashProperty.CurrentValue = hash;
            }
        }
    }
}
