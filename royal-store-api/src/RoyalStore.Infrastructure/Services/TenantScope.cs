using RoyalStore.Application.Abstractions.Services;

namespace RoyalStore.Infrastructure.Services;

/// <summary>
/// Ambient tenant for work that has no HTTP request behind it. Backed by an
/// <see cref="AsyncLocal{T}"/> so the value flows into everything awaited inside the scope,
/// including the DI scope and DbContext created there.
/// </summary>
/// <remarks>
/// Registered as a singleton: the AsyncLocal is per execution context, not per container scope, so
/// a single instance is both correct and necessary for the static accessor below.
/// </remarks>
public sealed class TenantScope : ITenantScope
{
    private static readonly AsyncLocal<Guid?> Current = new();

    /// <summary>The ambient tenant, or null when the caller is an ordinary HTTP request.</summary>
    internal static Guid? CurrentTenantId => Current.Value;

    public IDisposable Begin(Guid tenantId)
    {
        if (tenantId == Guid.Empty)
            throw new ArgumentException("Cannot begin a tenant scope for an empty tenant id.", nameof(tenantId));

        var previous = Current.Value;
        Current.Value = tenantId;
        return new Scope(previous);
    }

    /// <summary>Restores the previous value rather than clearing, so nested scopes behave.</summary>
    private sealed class Scope : IDisposable
    {
        private readonly Guid? _previous;
        private bool _disposed;

        public Scope(Guid? previous) => _previous = previous;

        public void Dispose()
        {
            if (_disposed)
                return;

            Current.Value = _previous;
            _disposed = true;
        }
    }
}
