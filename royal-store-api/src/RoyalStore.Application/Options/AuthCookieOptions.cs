using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Application.Options;

public sealed class AuthCookieOptions
{
    [Required]
    public string CookieName { get; init; } = "access_token";
}
