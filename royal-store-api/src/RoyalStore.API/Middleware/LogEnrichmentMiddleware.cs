using RoyalStore.Application.Abstractions.Services;
using Serilog;
using Serilog.Context;

namespace RoyalStore.API.Middleware;

public sealed class LogEnrichmentMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        IDiagnosticContext diagnosticContext)
    {
        var tenantId = tenantContext.TenantId == Guid.Empty
            ? (Guid?)null
            : tenantContext.TenantId;
        var userId = currentUserService.CurrentUserId;
        var requestId = context.TraceIdentifier;
        var endpoint = context.Request.Path.Value;

        // IDiagnosticContext.Set targets the single completion event written by
        // UseSerilogRequestLogging (registered earlier in the pipeline, before
        // UseAuthentication) — LogContext.PushProperty alone would be popped
        // before that outer middleware logs, since it sits outside this scope.
        diagnosticContext.Set("TenantId", tenantId);
        diagnosticContext.Set("UserId", userId);
        diagnosticContext.Set("RequestId", requestId);
        diagnosticContext.Set("Endpoint", endpoint);

        using (LogContext.PushProperty("TenantId", tenantId))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("RequestId", requestId))
        using (LogContext.PushProperty("Endpoint", endpoint))
        {
            await next(context);
        }
    }
}
