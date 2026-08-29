using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Permissions.Queries.GetPermissionCatalog;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.API.Endpoints;

public sealed class PermissionEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/permissions").WithTags("Permissions");

        routes.MapGet("", GetCatalog)
            .RequireAuthorization(PolicyNames.Permission(Permission.UsersManage));
    }

    private static async Task<IResult> GetCatalog(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPermissionCatalogQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
