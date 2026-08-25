using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Products.Commands.AddProductPhoto;
using RoyalStore.Application.Products.Commands.AssignProductSizes;
using RoyalStore.Application.Products.Commands.CreateProduct;
using RoyalStore.Application.Products.Commands.DisableProduct;
using RoyalStore.Application.Products.Commands.RemoveProductPhoto;
using RoyalStore.Application.Products.Commands.SetProductCoverPhoto;
using RoyalStore.Application.Products.Commands.UpdateProduct;
using RoyalStore.Application.Products.Queries.GetProductById;
using RoyalStore.Application.Products.Queries.GetProducts;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class ProductEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/products")
            .WithTags("Products")
            .RequireAuthorization(PolicyNames.AdminOrManager);

        routes.MapPost("", Create);
        routes.MapPut("{id:guid}", Update);
        routes.MapPatch("{id:guid}/disable", Disable);
        routes.MapGet("", GetAll);
        routes.MapGet("{id:guid}", GetById);

        routes.MapPut("{id:guid}/sizes", AssignSizes);
        routes.MapPost("{id:guid}/photos", AddPhoto);
        routes.MapDelete("{id:guid}/photos/{photoId:guid}", RemovePhoto);
        routes.MapPatch("{id:guid}/photos/{photoId:guid}/cover", SetCoverPhoto);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateProductCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/products/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        IMediator mediator,
        UpdateProductCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Disable(Guid id, IMediator mediator, CancellationToken ct)
    {
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
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> AssignSizes(
        Guid id,
        IMediator mediator,
        AssignProductSizesCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { ProductId = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> AddPhoto(
        Guid id,
        IMediator mediator,
        AddProductPhotoCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { ProductId = id }, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/products/{id}/photos/{r.Id}", r));
    }

    private static async Task<IResult> RemovePhoto(
        Guid id,
        Guid photoId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new RemoveProductPhotoCommand(id, photoId), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> SetCoverPhoto(
        Guid id,
        Guid photoId,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SetProductCoverPhotoCommand(id, photoId), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
