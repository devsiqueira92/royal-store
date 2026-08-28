using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Sellers.Commands.DeactivateSeller;
using RoyalStore.Application.Sellers.Commands.UpdateSeller;
using RoyalStore.Application.Sellers.Queries.GetSellerById;
using RoyalStore.Application.Sellers.Queries.GetSellers;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class SellerEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/sellers").WithTags("Sellers");

        routes.MapGet("", GetAll).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapGet("{id:guid}", GetById).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapPut("onboarding", Onboard).RequireAuthorization(PolicyNames.SellerOnboarding);
        routes.MapPatch("{id:guid}/deactivate", Deactivate).RequireAuthorization(PolicyNames.Admin);
    }

    private static async Task<IResult> GetAll(
        bool? isActive,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetSellersQuery(isActive), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSellerByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Onboard(
        IMediator mediator,
        UpdateSellerCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Deactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateSellerCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }
}
