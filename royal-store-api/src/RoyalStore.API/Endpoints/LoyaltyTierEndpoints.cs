using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Loyalty.Commands.ActivateLoyaltyTier;
using RoyalStore.Application.Loyalty.Commands.CreateLoyaltyTier;
using RoyalStore.Application.Loyalty.Commands.DeactivateLoyaltyTier;
using RoyalStore.Application.Loyalty.Commands.DeleteLoyaltyTier;
using RoyalStore.Application.Loyalty.Commands.UpdateLoyaltyTier;
using RoyalStore.Application.Loyalty.Queries.GetLoyaltyTierById;
using RoyalStore.Application.Loyalty.Queries.GetLoyaltyTiers;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class LoyaltyTierEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/loyalty-tiers")
            .WithTags("LoyaltyTiers");

        routes.MapPost("", Create).RequireAuthorization(PolicyNames.Admin);
        routes.MapPut("{id:guid}", Update).RequireAuthorization(PolicyNames.Admin);
        routes.MapDelete("{id:guid}", Delete).RequireAuthorization(PolicyNames.Admin);
        routes.MapPatch("{id:guid}/activate", Activate).RequireAuthorization(PolicyNames.Admin);
        routes.MapPatch("{id:guid}/deactivate", Deactivate).RequireAuthorization(PolicyNames.Admin);

        routes.MapGet("", GetAll).RequireAuthorization(PolicyNames.StaffRead);
        routes.MapGet("{id:guid}", GetById).RequireAuthorization(PolicyNames.StaffRead);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateLoyaltyTierCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/loyalty-tiers/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        IMediator mediator,
        UpdateLoyaltyTierCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteLoyaltyTierCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> Activate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateLoyaltyTierCommand(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Deactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateLoyaltyTierCommand(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLoyaltyTiersQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLoyaltyTierByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
