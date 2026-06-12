using System.Security.Claims;
using RoyalStore.Application.Abstractions.Security;

namespace RoyalStore.API.Authorization;

public sealed class AuthorizationAuditMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(async () =>
        {
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden
                && context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
                var tenantId = Guid.TryParse(context.User.FindFirstValue("tenantId"), out var tid)
                    ? tid
                    : Guid.Empty;

                var obfuscatedId = userId[..Math.Min(userId.Length, 50)];
                var failureReason = $"Policy denied for path '{context.Request.Path}'.";

                using var scope = context.RequestServices
                    .GetRequiredService<IServiceScopeFactory>()
                    .CreateScope();

                var auditService = scope.ServiceProvider.GetRequiredService<ISecurityAuditService>();
                await auditService.RecordAsync("authz.policy_denied", tenantId, failureReason, obfuscatedId);
            }
        });

        await next(context);
    }
}
