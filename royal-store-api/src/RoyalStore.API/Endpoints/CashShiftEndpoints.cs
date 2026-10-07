using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Sales.Commands.CloseCashShift;
using RoyalStore.Application.Sales.Commands.OpenCashShift;
using RoyalStore.Application.Sales.Queries.GetCurrentCashShift;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

public sealed class CashShiftEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/cash-shifts").WithTags("CashShifts");

        routes.MapPost("", Open).RequireAuthorization(PolicyNames.Admin);
        routes.MapPatch("close", Close).RequireAuthorization(PolicyNames.Admin);
        routes.MapGet("current", GetCurrent).RequireAuthorization(PolicyNames.Admin);
    }

    private static async Task<IResult> Open(
        OpenCashShiftCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/cash-shifts/{r.Id}", r));
    }

    private static async Task<IResult> Close(
        CloseCashShiftCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetCurrent(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCurrentCashShiftQuery(), ct);
        return result.ToHttpResult(r => r is null ? TypedResults.NoContent() : TypedResults.Ok(r));
    }
}
