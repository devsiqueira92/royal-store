using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Infrastructure.Authentication;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TokenResponse GenerateToken(
        UserEntity user,
        string role,
        bool onboardingCompleted,
        bool isProfileComplete,
        IReadOnlyCollection<string> permissions)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role),
            new("tenantId", user.TenantId.ToString()),
            new("onboardingStatus", onboardingCompleted ? "completed" : "pending"),
            new("isProfileComplete", isProfileComplete ? "true" : "false", ClaimValueTypes.Boolean),
        };

        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
