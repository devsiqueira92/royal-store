using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.TaxProfileTemplates.Commands.CreateTaxProfileTemplate;
using RoyalStore.Application.TaxProfileTemplates.Commands.DeactivateTaxProfileTemplate;
using RoyalStore.Application.TaxProfileTemplates.Commands.UpdateTaxProfileTemplate;
using RoyalStore.Application.TaxProfileTemplates.Queries.GetTaxProfileTemplates;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.API.Endpoints;

public sealed class TaxProfileTemplateEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/tax-profile-templates")
            .WithTags("TaxProfileTemplates")
            .RequireAuthorization(PolicyNames.Permission(Permission.CatalogManageTaxProfiles));

        routes.MapPost("", Create);
        routes.MapPut("{id:guid}", Update);
        routes.MapPatch("{id:guid}/deactivate", Deactivate);
        routes.MapGet("", GetAll);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateTaxProfileTemplateCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/tax-profile-templates/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        IMediator mediator,
        UpdateTaxProfileTemplateCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Deactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateTaxProfileTemplateCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> GetAll(int? type, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetTaxProfileTemplatesQuery((ItemType?)type), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
