namespace RoyalStore.Application.Abstractions.Services;

/// <summary>
/// Runs work as a given tenant where no HTTP request supplies one — in practice, a background
/// service iterating every store.
/// </summary>
/// <remarks>
/// This exists because <see cref="ITenantContext"/> is normally resolved from the caller's JWT. A
/// BackgroundService has no HttpContext, so without an ambient tenant every global query filter
/// would match nothing and the worker would silently see an empty database (research Decision 1).
/// Scoping this way KEEPS the query filter as the single isolation mechanism, which is what
/// Constitution V requires — the alternative, IgnoreQueryFilters plus a hand-written TenantId
/// predicate, is exactly the manual per-query filtering it prohibits.
/// </remarks>
public interface ITenantScope
{
    /// <summary>Makes <paramref name="tenantId"/> the ambient tenant until the returned scope is disposed.</summary>
    IDisposable Begin(Guid tenantId);
}
