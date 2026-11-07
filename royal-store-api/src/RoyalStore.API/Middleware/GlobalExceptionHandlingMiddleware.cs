using Microsoft.EntityFrameworkCore;

namespace RoyalStore.API.Middleware;

public sealed class GlobalExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (DbUpdateConcurrencyException)
        {
            // Defense-in-depth against a genuine concurrent save race that the
            // application-layer version comparison couldn't catch (both requests
            // loaded the same "current" version moments apart). Mirrors the shape
            // ToHttpResult's MapError produces for Error.Conflict.
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new
                {
                    code = "product.stale",
                    message = "The product was modified by someone else. Reload and try again."
                });
            }
        }
        catch (Exception ex)
        {
      logger.LogError(
                ex,
                "Unhandled exception for request {Method} {Path} | TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);

            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc7807",
                    title = "An unexpected error occurred.",
                    status = 500,
                    traceId = context.TraceIdentifier
                });
            }
        }
    }
}
