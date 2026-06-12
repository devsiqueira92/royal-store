using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Categories.Commands.CreateCategory;
using RoyalStore.Application.Categories.Commands.DisableCategory;
using RoyalStore.Application.Categories.Queries.GetCategories;
using RoyalStore.Application.Categories.Queries.GetCategoryById;
using RoyalStore.Application.Categories.Commands.UpdateCategory;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class CategoryEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/categories")
            .WithTags("Categories")
            .RequireAuthorization(PolicyNames.Admin);

        routes.MapPost("", Create);
        routes.MapPut("{id:guid}", Update);
        routes.MapPatch("{id:guid}/disable", Disable);
        routes.MapGet("", GetAll);
        routes.MapGet("{id:guid}", GetById);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateCategoryCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/categories/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        IMediator mediator,
        UpdateCategoryCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Disable(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DisableCategoryCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoriesQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCategoryByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
