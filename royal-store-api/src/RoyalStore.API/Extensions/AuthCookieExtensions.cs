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

    // Deliberately not HttpOnly/Secure: this cookie carries no secret, only a
    // UI hint, so client-side scripts must be able to read it (spec FR-002/FR-003).
    public static void SetLoggedInCookie(
        this HttpResponse response,
        DateTimeOffset expiresAt,
        AuthCookieOptions options)
    {
        response.Cookies.Append(options.IsLoggedInCookieName, "true", new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt,
            Path = "/",
        });
    }

    public static void ClearLoggedInCookie(
        this HttpResponse response,
        AuthCookieOptions options)
    {
        response.Cookies.Delete(options.IsLoggedInCookieName, new CookieOptions
        {
            HttpOnly = false,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/",
        });
    }
}
