using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Labels.Commands.CreateLabel;
using RoyalStore.Application.Labels.Commands.DeleteLabel;
using RoyalStore.Application.Labels.Commands.SetLabelActive;
using RoyalStore.Application.Labels.Commands.UpdateLabel;
using RoyalStore.Application.Labels.Queries.GetLabelById;
using RoyalStore.Application.Labels.Queries.GetLabelConfigurations;
using RoyalStore.Application.Labels.Queries.GetLabelOptions;
using RoyalStore.Application.Labels.Queries.GetLabelPrintOptions;
using RoyalStore.Application.Labels.Queries.GetLabels;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

/// <summary>
/// The labels the store saved from the designer (065): a template and the format it is printed on,
/// identified by the template's id. Reading is open to all staff, since printing labels is routine
/// shop-floor work; changing them is for administrators and managers — no permission of its own
/// (065 plan, Complexity Tracking).
/// </summary>
public sealed class LabelEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/labels").WithTags("Labels");

        routes.MapGet("", GetAll).RequireAuthorization(PolicyNames.StaffRead);

        // Active labels only, as { code, description } for selection lists. Declared before
        // "{id:guid}" for readability; the route constraint already keeps them apart.
        routes.MapGet("options", GetOptions).RequireAuthorization(PolicyNames.StaffRead);

        // System configurations + the store's active labels, for the designer's "start from" list (066).
        routes.MapGet("configurations", GetConfigurations).RequireAuthorization(PolicyNames.StaffRead);

        // Store's active labels (default first) + system configurations, for the print screen's label list.
        routes.MapGet("print-options", GetPrintOptions).RequireAuthorization(PolicyNames.StaffRead);
        routes.MapGet("{id:guid}", GetById).RequireAuthorization(PolicyNames.StaffRead);

        routes.MapPost("", Create).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapPut("{id:guid}", Update).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapPatch("{id:guid}/activate", Activate).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapPatch("{id:guid}/deactivate", Deactivate).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapDelete("{id:guid}", Delete).RequireAuthorization(PolicyNames.AdminOrManager);
    }

    private static async Task<IResult> GetOptions(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLabelOptionsQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetConfigurations(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLabelConfigurationsQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetPrintOptions(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLabelPrintOptionsQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Activate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetLabelActiveCommand(id, true), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> Deactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetLabelActiveCommand(id, false), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> Delete(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteLabelCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> Update(
        Guid id, UpdateLabelCommand command, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetAll(
        [FromQuery(Name = "is-active")] bool? isActive,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetLabelsQuery(isActive), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetLabelByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Create(
        CreateLabelCommand command, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/labels/{r.Template.Id}", r));
    }
}
