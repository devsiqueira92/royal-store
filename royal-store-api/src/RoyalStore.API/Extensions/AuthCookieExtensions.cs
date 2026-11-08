using RoyalStore.Application.Options;

namespace RoyalStore.API.Extensions;

public static class AuthCookieExtensions
{
    public static void SetAuthCookie(
        this HttpResponse response,
        string token,
        DateTimeOffset expiresAt,
        AuthCookieOptions options)
    {
        response.Cookies.Append(options.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            // Frontend and API are different origins (SPA on one host, API on another), so every
            // request is cross-site. SameSite=None is the only mode browsers deliver the cookie on,
            // and it's only accepted together with Secure — never tie this to IsProduction().
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt,
            Path = "/",
        });
    }

    public static void ClearAuthCookie(
        this HttpResponse response,
        AuthCookieOptions options)
    {
        response.Cookies.Delete(options.CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
        });
    }

    // Deliberately not HttpOnly: this cookie carries no secret, only a UI hint, so client-side
    // scripts must be able to read it (spec FR-002/FR-003). Secure/SameSite still need to match
    // SetAuthCookie's, or the browser won't deliver it cross-site either.
    public static void SetLoggedInCookie(
        this HttpResponse response,
        DateTimeOffset expiresAt,
        AuthCookieOptions options)
    {
        response.Cookies.Append(options.IsLoggedInCookieName, "true", new CookieOptions
        {
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.None,
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
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
        });
    }
}
