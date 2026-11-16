 using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
using RoyalStore.Application.PriceLists.Commands.ClearPriceListRule;
using RoyalStore.Application.PriceLists.Commands.CreatePriceList;
using RoyalStore.Application.PriceLists.Commands.DeletePriceList;
using RoyalStore.Application.PriceLists.Commands.RenamePriceList;
using RoyalStore.Application.PriceLists.Commands.SetDefaultPriceList;
using RoyalStore.Application.PriceLists.Commands.SetPriceListActive;
using RoyalStore.Application.PriceLists.Commands.SetPriceListRule;
using RoyalStore.Application.PriceLists.Queries.GetPriceLists;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.API.Endpoints;

/// <summary>
/// The store's price lists (061). Reading them is open to whoever can maintain products — the
/// product form needs the lists to price against. Changing them is gated by its own permission:
/// switching the default re-prices the whole store.
/// </summary>
public sealed class PriceListEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/price-lists").WithTags("PriceLists");

        routes.MapGet("", GetAll)
            .RequireAuthorization(PolicyNames.AdminOrManager);

        var manage = PolicyNames.Permission(Permission.CatalogManagePriceLists);

        routes.MapPost("", Create).RequireAuthorization(manage);
        routes.MapPut("{id:guid}", Rename).RequireAuthorization(manage);
        routes.MapPatch("{id:guid}/default", SetDefault).RequireAuthorization(manage);
        routes.MapPatch("{id:guid}/activate", Activate).RequireAuthorization(manage);
        routes.MapPatch("{id:guid}/deactivate", Deactivate).RequireAuthorization(manage);
        routes.MapDelete("{id:guid}", Delete).RequireAuthorization(manage);

        // The percentage rule (062) has its own routes on purpose: the rename route above takes a
        // name and nothing else, so a client that knows nothing about rules cannot wipe one.
        routes.MapPut("{id:guid}/rule", SetRule).RequireAuthorization(manage);
        routes.MapDelete("{id:guid}/rule", ClearRule).RequireAuthorization(manage);
    }

    private static async Task<IResult> GetAll(
        [FromQuery(Name = "is-active")] bool? isActive,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetPriceListsQuery(isActive), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Create(
        CreatePriceListCommand command, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/price-lists/{r.Id}", r));
    }

    private static async Task<IResult> Rename(
        Guid id, RenamePriceListCommand command, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> SetRule(
        Guid id, SetPriceListRuleCommand command, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> ClearRule(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new ClearPriceListRuleCommand(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> SetDefault(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetDefaultPriceListCommand(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Activate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetPriceListActiveCommand(id, true), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> Deactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetPriceListActiveCommand(id, false), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeletePriceListCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }
}
