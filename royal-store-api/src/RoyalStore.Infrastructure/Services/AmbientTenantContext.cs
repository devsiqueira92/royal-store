using Microsoft.AspNetCore.Http;
using RoyalStore.Application.Abstractions.Services;

namespace RoyalStore.Infrastructure.Services;

/// <summary>
/// The registered <see cref="ITenantContext"/>. Returns the ambient tenant when a
/// <see cref="TenantScope"/> is open, and otherwise falls back to the caller's JWT claim exactly as
/// <see cref="JwtTenantContext"/> always did.
/// </summary>
/// <remarks>
/// <para>
/// Introduced for the expense recurrence worker (060 research Decision 1), which runs with no
/// HttpContext: without this, its tenant would be <see cref="Guid.Empty"/>, every global query
/// filter would match nothing, and the worker would quietly generate no bills while reporting
/// success.
/// </para>
/// <para>
/// An ordinary HTTP request opens no scope, so it takes the JWT branch and behaves identically to
/// before this type existed. That is the whole reason the ambient value is checked first and
/// nothing else changed.
/// </para>
/// </remarks>
public sealed class AmbientTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AmbientTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var ambient = TenantScope.CurrentTenantId;
            if (ambient is not null)
                return ambient.Value;

            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("tenantId")?.Value;

            return Guid.TryParse(claim, out var tenantId) ? tenantId : Guid.Empty;
        }
    }
}
