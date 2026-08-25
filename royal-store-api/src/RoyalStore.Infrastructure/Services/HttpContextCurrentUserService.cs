using Microsoft.AspNetCore.Http;
using RoyalStore.Application.Abstractions.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RoyalStore.Infrastructure.Services;

internal sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? CurrentUserId =>
    _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
}
 