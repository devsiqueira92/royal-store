using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Sales.Commands.SyncSales;
using RoyalStore.Application.Sales.Queries.GetPosProducts;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class SaleEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/sales").WithTags("Sales");
        routes.MapPost("sync", Sync).RequireAuthorization(PolicyNames.Seller);
        routes.MapGet("products", GetProducts).RequireAuthorization(PolicyNames.Admin);
    }

    private static async Task<IResult> Sync(
        SyncSalesCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetProducts(
        [FromQuery(Name = "category-id")] Guid? categoryId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPosProductsQuery(categoryId), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
