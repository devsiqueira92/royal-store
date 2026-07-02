using RoyalStore.API.Endpoints;
using RoyalStore.Domain.Common;

namespace RoyalStore.API.Extensions;

public static class EndpointExtensions
{
    public static WebApplication RegisterEndpointDefinitions(this WebApplication app)
    {
        var definitions = typeof(Program).Assembly
            .GetTypes()
            .Where(t => typeof(IEndpointDefinition).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (IEndpointDefinition)Activator.CreateInstance(t)!);

        foreach (var definition in definitions)
            definition.RegisterEndpoints(app);

        return app;
    }

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess
            ? onSuccess(result.Value)
            : MapError(result.Error);

    public static IResult ToHttpResult(this Result result, Func<IResult> onSuccess) =>
        result.IsSuccess
            ? onSuccess()
            : MapError(result.Error);

    private static IResult MapError(Error error) => error.Type switch
    {
        ErrorType.Validation => Results.BadRequest(new { error.Code, error.Message }),
        ErrorType.Conflict => Results.Conflict(new { error.Code, error.Message }),
        ErrorType.Unauthorized => Results.Json(
            new { error.Code, error.Message },
            statusCode: StatusCodes.Status401Unauthorized),
        ErrorType.NotFound => Results.NotFound(new { error.Code, error.Message }),
        ErrorType.RateLimit => Results.StatusCode(429),
        _ => Results.Problem(error.Message)
    };
}
