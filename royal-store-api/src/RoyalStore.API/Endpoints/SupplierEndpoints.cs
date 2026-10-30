using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Suppliers.Commands.ActivateSupplier;
using RoyalStore.Application.Suppliers.Commands.CreateSupplier;
using RoyalStore.Application.Suppliers.Commands.DeactivateSupplier;
using RoyalStore.Application.Suppliers.Commands.UpdateSupplier;
using RoyalStore.Application.Suppliers.Queries.GetSupplierById;
using RoyalStore.Application.Suppliers.Queries.GetSuppliers;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class SupplierEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/suppliers")
            .WithTags("Suppliers")
            .RequireAuthorization(PolicyNames.AdminOrManager);

        routes.MapPost("", Create);
        routes.MapPut("{id:guid}", Update);
        routes.MapPatch("{id:guid}/activate", Activate);
        routes.MapPatch("{id:guid}/deactivate", Deactivate);
        routes.MapGet("", GetAll);
        routes.MapGet("{id:guid}", GetById);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateSupplierCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/suppliers/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        IMediator mediator,
        UpdateSupplierCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Activate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivateSupplierCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> Deactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateSupplierCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> GetAll(bool? isActive, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSuppliersQuery(isActive), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetSupplierByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
