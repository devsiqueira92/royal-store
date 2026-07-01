using RoyalStore.Application.Options;

namespace RoyalStore.API.Extensions;

public static class AuthCookieExtensions
{
    public static void SetAuthCookie(
        this HttpResponse response,
        string token,
        DateTimeOffset expiresAt,
        IWebHostEnvironment environment,
        AuthCookieOptions options)
    {
        response.Cookies.Append(options.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = environment.IsProduction(),
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            Path = "/",
        });
    }

    public static void ClearAuthCookie(
        this HttpResponse response,
        IWebHostEnvironment environment,
        AuthCookieOptions options)
    {
        response.Cookies.Delete(options.CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = environment.IsProduction(),
            SameSite = SameSiteMode.Strict,
            Path = "/",
        });
    }
}
