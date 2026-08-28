using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Users.Commands.CreateUser;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class UserEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/users")
            .WithTags("Users");

        routes.MapPost("", CreateUser)
            .RequireAuthorization(PolicyNames.Admin);
    }

    private static async Task<IResult> CreateUser(
        IMediator mediator,
        CreateUserCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created("/api/users", r));
    }
}
