using Microsoft.AspNetCore.Http;
using RoyalStore.Application.Abstractions.Services;

namespace RoyalStore.Infrastructure.Services;

internal sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CurrentUserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
}
