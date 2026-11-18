using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.CostCompositions.Commands.CreateCostComposition;
using RoyalStore.Application.CostCompositions.Commands.DeleteCostComposition;
using RoyalStore.Application.CostCompositions.Commands.UpdateCostComposition;
using RoyalStore.Application.CostCompositions.Queries.GetCostCompositionById;
using RoyalStore.Application.CostCompositions.Queries.GetCostCompositions;
using RoyalStore.Application.CostCompositions.Queries.GetUnlinkedCostCompositions;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

/// <summary>
/// Cost composition lists (064): a product's cost breakdown as a list of its own. A list is created
/// first and tied to a product by a product save that names its identifier. Same access rule as the
/// product routes — whoever can maintain products can maintain these.
/// </summary>
public sealed class CostCompositionEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/cost-compositions")
            .WithTags("CostCompositions")
            .RequireAuthorization(PolicyNames.AdminOrManager);

        routes.MapPost("", Create);
        routes.MapGet("", GetAll);

        // Declared before "{id:guid}" for readability; the route constraint already keeps them apart.
        routes.MapGet("unlinked", GetUnlinked);
        routes.MapGet("{id:guid}", GetById);
        routes.MapPut("{id:guid}", Update);
        routes.MapDelete("{id:guid}", Delete);
    }

    private static async Task<IResult> Create(
        CreateCostCompositionCommand command, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/cost-compositions/{r.Id}", r));
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCostCompositionsQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCostCompositionByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetUnlinked(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnlinkedCostCompositionsQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Update(
        Guid id, UpdateCostCompositionCommand command, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCostCompositionCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }
}
