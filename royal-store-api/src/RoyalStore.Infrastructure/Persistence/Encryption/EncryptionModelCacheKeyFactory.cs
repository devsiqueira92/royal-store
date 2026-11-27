using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using RoyalStore.Application.Abstractions.Security;

namespace RoyalStore.Infrastructure.Persistence.Encryption;

/// <summary>A context whose model's value converters capture an <see cref="IEncryptionService"/>.</summary>
public interface IEncryptedDbContext
{
    IEncryptionService Encryption { get; }
}

/// <summary>
/// EF caches one model per context type by default, so the converters would keep whichever
/// <see cref="IEncryptionService"/> built the first model — a different key set (another host,
/// a test, a rotation) would silently reuse the old one. Keying the cache on the service instance
/// prevents that; with the production singleton there is still exactly one model.
/// Register with <c>optionsBuilder.ReplaceService&lt;IModelCacheKeyFactory, EncryptionModelCacheKeyFactory&gt;()</c>.
/// </summary>
public sealed class EncryptionModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime) =>
        (context.GetType(), (context as IEncryptedDbContext)?.Encryption, designTime);
}
