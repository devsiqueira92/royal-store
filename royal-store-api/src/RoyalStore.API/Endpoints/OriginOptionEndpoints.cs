using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.OriginOptions.Commands.CreateOriginOption;
using RoyalStore.Application.OriginOptions.Commands.UpdateOriginOption;
using RoyalStore.Application.OriginOptions.Queries.GetOriginOptionByCode;
using RoyalStore.Application.OriginOptions.Queries.GetOriginOptions;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class OriginOptionEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/origin-options").WithTags("OriginOptions");

        // Writes are more restrictive than reads — this is global, shared data (FR-017/FR-019).
        routes.MapPost("", Create).RequireAuthorization(PolicyNames.SuperAdmin);
        routes.MapPut("{code:int}", Update).RequireAuthorization(PolicyNames.SuperAdmin);
        routes.MapGet("", GetAll).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapGet("{code:int}", GetByCode).RequireAuthorization(PolicyNames.AdminOrManager);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateOriginOptionCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/origin-options/{r.Code}", r));
    }

    private static async Task<IResult> Update(
        string code,
        IMediator mediator,
        UpdateOriginOptionCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { CurrentCode = code }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOriginOptionsQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetByCode(string code, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOriginOptionByCodeQuery(code), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
