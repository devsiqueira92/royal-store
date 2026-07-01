using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
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
        var routes = app.MapGroup("api/auth").WithTags("Auth").AllowAnonymous();

        routes.MapPost("register", Register);
        routes.MapGet("verify-email", VerifyEmail);
        routes.MapPost("login", Login);
        routes.MapPost("forgot-password", ForgotPassword);
        routes.MapPost("reset-password", ResetPassword);
        routes.MapPost("resend-confirmation-email", ResendConfirmationEmail)
            .RequireRateLimiting("resend-confirmation-email");
    }

    private static async Task<IResult> Register(IMediator mediator, RegisterTenantCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/tenants/{r.TenantId}", r));
    }

    private static async Task<IResult> Login(IMediator mediator, LoginCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(token => TypedResults.Ok(token));
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
}
