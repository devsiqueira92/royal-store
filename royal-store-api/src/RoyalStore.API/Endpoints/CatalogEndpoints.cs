using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Catalog.Queries.GetCatalog;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class CatalogEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/catalog").WithTags("Catalog");
        routes.MapGet("", GetCatalog).RequireAuthorization(PolicyNames.Seller);
    }

    private static async Task<IResult> GetCatalog(
        DateTimeOffset? since,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCatalogQuery(since), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
