using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Loyalty.Commands.ReverseSaleCashback;
using RoyalStore.Application.Sales.Commands.RegisterSale;
using RoyalStore.Application.Sales.Queries.GetPosProducts;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.API.Endpoints;

public sealed class SaleEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/sales").WithTags("Sales");
        routes.MapPost("", RegisterSale).RequireAuthorization(PolicyNames.Admin);
        routes.MapGet("products", GetProducts).RequireAuthorization(PolicyNames.Admin);

        routes.MapPost("{id:guid}/cashback/reversal", ReverseCashback)
            .RequireAuthorization(PolicyNames.Permission(Permission.SalesRefund));
    }

    private static async Task<IResult> RegisterSale (
        RegisterSaleCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> ReverseCashback(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new ReverseSaleCashbackCommand(id), ct);
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
