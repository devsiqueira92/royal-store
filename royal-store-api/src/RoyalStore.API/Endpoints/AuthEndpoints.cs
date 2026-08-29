using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Auth.Commands.RefreshToken;
using RoyalStore.Application.Options;
using RoyalStore.Application.Tenants.Commands.ChangePendingEmail;
using RoyalStore.Application.Tenants.Commands.ForgotPassword;
using RoyalStore.Application.Tenants.Commands.Login;
using RoyalStore.Application.Tenants.Commands.RegisterTenant;
using RoyalStore.Application.Tenants.Commands.ResendConfirmationEmail;
using RoyalStore.Application.Tenants.Commands.ResetPassword;
using RoyalStore.Application.Tenants.Commands.VerifyEmail;

namespace RoyalStore.API.Endpoints;

public sealed class AuthEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        // Deliberately NOT AllowAnonymous() at the group level: ASP.NET Core's
        // AllowAnonymous metadata short-circuits authorization for every route
        // in the group, even ones that later call .RequireAuthorization() —
        // so each anonymous route below opts in individually instead, leaving
        // "session" as the group's only authenticated route.
        var routes = app.MapGroup("api/auth").WithTags("Auth");

        routes.MapPost("register", Register).AllowAnonymous();
        routes.MapGet("verify-email", VerifyEmail).AllowAnonymous();
        routes.MapPost("login", Login).AllowAnonymous();
        routes.MapPost("forgot-password", ForgotPassword).AllowAnonymous();
        routes.MapPost("reset-password", ResetPassword).AllowAnonymous();
        routes.MapPost("resend-confirmation-email", ResendConfirmationEmail)
            .AllowAnonymous()
            .RequireRateLimiting("resend-confirmation-email");
        routes.MapPost("change-pending-email", ChangePendingEmail)
            .AllowAnonymous()
            .RequireRateLimiting("change-pending-email");
        routes.MapPost("logout", Logout).AllowAnonymous();
        routes.MapPost("refresh", Refresh).RequireAuthorization();
        routes.MapGet("session", Session).RequireAuthorization();
    }

    private static async Task<IResult> Register(IMediator mediator, RegisterTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/tenants/{r.TenantId}", r));
    }

    private static async Task<IResult> Login(
        IMediator mediator,
        LoginCommand command,
        HttpContext httpContext,
        IWebHostEnvironment environment,
        IOptions<AuthCookieOptions> authCookieOptions,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(token =>
        {
            httpContext.Response.SetAuthCookie(token.Token, token.ExpiresAt, environment, authCookieOptions.Value);
            httpContext.Response.SetLoggedInCookie(token.ExpiresAt, authCookieOptions.Value);
            return TypedResults.Ok(new { Message = "Login successful." });
        });
    }

    private static async Task<IResult> VerifyEmail(
       [FromQuery] string userId,
       [FromQuery] string token,
       IMediator mediator,
       CancellationToken ct)
    {
        var result = await mediator.Send(new VerifyEmailCommand(userId, token), ct);
        return result.ToHttpResult(() => TypedResults.Ok(new { Message = "Email verified successfully. You can now log in." }));
    }

    private static async Task<IResult> ForgotPassword(
        IMediator mediator,
        ForgotPasswordCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(() => TypedResults.Ok(new
        {
            Message = "If an account with this email exists, a password reset link has been sent."
        }));
    }

    private static async Task<IResult> ResetPassword(
        IMediator mediator,
        ResetPasswordCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(() => TypedResults.Ok(new
        {
            Message = "Your password has been reset successfully. You can now log in."
        }));
    }

    private static async Task<IResult> ResendConfirmationEmail(
        IMediator mediator,
        ResendConfirmationEmailCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(() => TypedResults.Ok(new
        {
            Message = "If an account with this email exists and is not yet confirmed, a new confirmation email has been sent."
        }));
    }

    private static async Task<IResult> ChangePendingEmail(
        IMediator mediator,
        ChangePendingEmailCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(() => TypedResults.Ok(new
        {
            Message = "Your email has been updated. A new confirmation email has been sent."
        }));
    }

    private static async Task<IResult> Refresh(
        IMediator mediator,
        HttpContext httpContext,
        IWebHostEnvironment environment,
        IOptions<AuthCookieOptions> authCookieOptions,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(), ct);
        return result.ToHttpResult(token =>
        {
            httpContext.Response.SetAuthCookie(token.Token, token.ExpiresAt, environment, authCookieOptions.Value);
            httpContext.Response.SetLoggedInCookie(token.ExpiresAt, authCookieOptions.Value);
            return TypedResults.Ok(new { Message = "Token refreshed." });
        });
    }

    private static IResult Logout(
        HttpContext httpContext,
        IWebHostEnvironment environment,
        IOptions<AuthCookieOptions> authCookieOptions)
    {
        httpContext.Response.ClearAuthCookie(environment, authCookieOptions.Value);
        httpContext.Response.ClearLoggedInCookie(authCookieOptions.Value);
        return TypedResults.Ok(new { Message = "Logged out successfully." });
    }

    private static IResult Session(ClaimsPrincipal user)
    {
        // The JWT Bearer handler's default inbound claim mapping rewrites some
        // standard short claim types (e.g. "email") to their long ClaimTypes.*
        // equivalent when building the ClaimsPrincipal, even though the token
        // itself was signed with the short JwtRegisteredClaimNames form.
        var email = user.FindFirstValue(ClaimTypes.Email);
        var tenantId = user.FindFirstValue("tenantId");
        var role = user.FindFirstValue(ClaimTypes.Role);
        var onboardingStatus = user.FindFirstValue("onboardingStatus");
        var permissions = user.FindAll("permission")
            .Select(c => c.Value)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToArray();

        return TypedResults.Ok(new
        {
            Email = email,
            TenantId = tenantId,
            Role = role,
            OnboardingStatus = onboardingStatus,
            Permissions = permissions,
        });
    }
}
