using Microsoft.AspNetCore.Http;
using RoyalStore.Application.Abstractions.Services;

namespace RoyalStore.Infrastructure.Services;

public sealed class JwtTenantContext : ITenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtTenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("tenantId")?.Value;

            return Guid.TryParse(claim, out var tenantId) ? tenantId : Guid.Empty;
        }
    }
}
