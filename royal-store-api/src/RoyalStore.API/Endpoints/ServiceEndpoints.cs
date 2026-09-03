using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Products.Commands.DisableProduct;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Application.Products.Queries.GetProductById;
using RoyalStore.Application.Products.Queries.GetProducts;
using RoyalStore.Application.Services.Commands.CreateService;
using RoyalStore.Application.Services.Commands.EditService;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class ServiceEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/services")
            .WithTags("Services")
            .RequireAuthorization(PolicyNames.AdminOrManager);

        routes.MapPost("", Create);
        routes.MapPut("{id:guid}", Update);
        routes.MapPatch("{id:guid}/disable", Disable);
        routes.MapGet("", GetAll);
        routes.MapGet("{id:guid}", GetById);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateServiceCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/services/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        IMediator mediator,
        EditServiceCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Disable(Guid id, IMediator mediator, CancellationToken ct)
    {
        var lookup = await mediator.Send(new GetProductByIdQuery(id), ct);
        if (!lookup.IsSuccess || lookup.Value.Type != (int)ItemType.Service)
            return Results.NotFound();

        var result = await mediator.Send(new DisableProductCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> GetAll(
        bool? isActive,
        Guid? categoryId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductsQuery(isActive, categoryId), ct);
        return result.ToHttpResult(r => TypedResults.Ok(FilterServices(r)));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        return result.ToHttpResult(r => r.Type == (int)ItemType.Service
            ? TypedResults.Ok(r)
            : Results.NotFound());
    }

    private static IReadOnlyList<ProductResponse> FilterServices(IReadOnlyList<ProductResponse> products) =>
        products.Where(p => p.Type == (int)ItemType.Service).ToList();
}
