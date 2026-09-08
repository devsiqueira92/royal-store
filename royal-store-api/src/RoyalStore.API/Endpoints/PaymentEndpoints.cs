using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Payments.Commands.ActivatePayment;
using RoyalStore.Application.Payments.Commands.DeactivatePayment;
using RoyalStore.Application.Payments.Commands.UpdatePaymentDiscount;
using RoyalStore.Application.Payments.Queries.GetPayments;
using RoyalStore.Domain.Const;

namespace RoyalStore.API.Endpoints;

/// <summary>
/// Tenant-scoped payment method catalog. There is deliberately no create or delete route — the
/// catalog is fully populated by seeding and payment methods are never removable (FR-004).
/// </summary>
public sealed class PaymentEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/payments").WithTags("Payments");

        routes.MapGet("", GetAll).RequireAuthorization(PolicyNames.StaffRead);

        routes.MapPatch("{id:guid}/discount", UpdateDiscount).RequireAuthorization(PolicyNames.Admin);
        routes.MapPatch("{id:guid}/activate", Activate).RequireAuthorization(PolicyNames.Admin);
        routes.MapPatch("{id:guid}/deactivate", Deactivate).RequireAuthorization(PolicyNames.Admin);
    }

    private static async Task<IResult> GetAll(IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new GetPaymentsQuery(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> UpdateDiscount(
        Guid id,
        IMediator mediator,
        UpdatePaymentDiscountCommand command,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Activate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new ActivatePaymentCommand(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Deactivate(Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivatePaymentCommand(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }
}
