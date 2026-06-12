using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Application.Options;

public sealed class PasswordResetOptions
{
    [Required]
    public string ClientAppBaseUrl { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int TokenExpiryMinutes { get; init; } = 60;

    [Range(1, 20)]
    public int MaxAttemptsPerWindow { get; init; } = 3;

    [Range(1, 1440)]
    public int WindowMinutes { get; init; } = 60;
}
