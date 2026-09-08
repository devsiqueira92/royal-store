using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Customers.Commands.CreateCustomer;
using RoyalStore.Application.Customers.Commands.DeactivateCustomer;
using RoyalStore.Application.Customers.Commands.ReactivateCustomer;
using RoyalStore.Application.Customers.Commands.UpdateCustomer;
using RoyalStore.Application.Customers.Queries.GetCustomerById;
using RoyalStore.Application.Customers.Queries.GetCustomerCashbackBalance;
using RoyalStore.Application.Customers.Queries.GetCustomerCashbackHistory;
using RoyalStore.Application.Customers.Queries.GetCustomers;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class CustomerEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/customers")
            .WithTags("Customers");

        routes.MapPost("", Create).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapPut("{id:guid}", Update).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapPatch("{id:guid}/deactivate", Deactivate).RequireAuthorization(PolicyNames.AdminOrManager);
        routes.MapPatch("{id:guid}/reactivate", Reactivate).RequireAuthorization(PolicyNames.AdminOrManager);

        routes.MapGet("", GetAll).RequireAuthorization(PolicyNames.StaffRead);
        routes.MapGet("{id:guid}", GetById).RequireAuthorization(PolicyNames.StaffRead);
        routes.MapGet("{id:guid}/cashback-balance", GetCashbackBalance).RequireAuthorization(PolicyNames.StaffRead);
        routes.MapGet("{id:guid}/cashback-history", GetCashbackHistory).RequireAuthorization(PolicyNames.StaffRead);
    }

    private static async Task<IResult> Create(
        IMediator mediator,
        CreateCustomerCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/customers/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        IMediator mediator,
        UpdateCustomerCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Deactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivateCustomerCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> Reactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new ReactivateCustomerCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomersQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetCashbackBalance(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerCashbackBalanceQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetCashbackHistory(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetCustomerCashbackHistoryQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
