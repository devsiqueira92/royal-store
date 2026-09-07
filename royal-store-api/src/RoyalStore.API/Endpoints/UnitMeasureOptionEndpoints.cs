using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.UnitMeasureOptions.Commands.CreateUnitMeasureOption;
using RoyalStore.Application.UnitMeasureOptions.Commands.UpdateUnitMeasureOption;
using RoyalStore.Application.UnitMeasureOptions.Queries.GetUnitMeasureOptionById;
using RoyalStore.Application.UnitMeasureOptions.Queries.GetUnitMeasureOptions;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class UnitMeasureOptionEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/unit-measure-options")
            .WithTags("UnitMeasureOptions")
            .RequireAuthorization(PolicyNames.AdminOrManager);

        routes.MapPost("", Create);
        routes.MapPut("{id:guid}", Update);
        routes.MapGet("", GetAll);
        routes.MapGet("{id:guid}", GetById);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateUnitMeasureOptionCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/unit-measure-options/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        IMediator mediator,
        UpdateUnitMeasureOptionCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnitMeasureOptionsQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnitMeasureOptionByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
