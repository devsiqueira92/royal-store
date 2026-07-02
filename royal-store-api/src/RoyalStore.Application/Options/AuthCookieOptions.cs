using System.ComponentModel.DataAnnotations;

namespace RoyalStore.Application.Options;

public sealed class AuthCookieOptions
{
    [Required]
    public string CookieName { get; init; } = "access_token";

    [Required]
    public string IsLoggedInCookieName { get; init; } = "is_logged_in";
}
