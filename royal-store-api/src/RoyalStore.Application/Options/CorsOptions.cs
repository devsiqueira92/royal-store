using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Application.Options;

public sealed class CorsOptions
{
    [Required, MinLength(1)]
    public string[] AllowedOrigins { get; init; } = [];
}
