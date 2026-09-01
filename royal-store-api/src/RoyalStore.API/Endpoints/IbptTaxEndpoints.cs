using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Fiscal.Queries.GetIbptTaxById;
using RoyalStore.Application.Fiscal.Queries.SearchIbptTaxes;

namespace RoyalStore.API.Endpoints;

public sealed class IbptTaxEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/ibpt-taxes").WithTags("IbptTaxes");

        routes.MapGet("", Search).RequireAuthorization();
        routes.MapGet("{id:guid}", GetById).RequireAuthorization();
    }

    private static async Task<IResult> Search(
        [FromQuery] string? q,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SearchIbptTaxesQuery(q), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetIbptTaxByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
