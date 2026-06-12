using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Infrastructure.Authentication;

public sealed class JwtOptions
{
    [Required]
    [MinLength(32)]
    public string SecretKey { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(1, 10080)]
    public int ExpiryMinutes { get; init; } = 60;
}
