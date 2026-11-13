using MediatR;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Expenses.Commands.CreateExpenseRecurrence;
using RoyalStore.Application.Expenses.Commands.DeleteExpenseRecurrence;
using RoyalStore.Application.Expenses.Commands.GenerateRecurringExpenses;
using RoyalStore.Application.Expenses.Commands.SetExpenseRecurrenceActive;
using RoyalStore.Application.Expenses.Commands.UpdateExpenseRecurrence;
using RoyalStore.Application.Expenses.Queries.GetExpenseRecurrences;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.API.Endpoints;

/// <summary>Recurring operating-expense templates and the on-demand generation trigger.</summary>
public sealed class ExpenseRecurrenceEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/expense-recurrences").WithTags("ExpenseRecurrences");

        routes.MapGet("", GetAll)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesView));

        routes.MapPost("", Create)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageRecurrences));

        routes.MapPost("generate", Generate)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageRecurrences));

        routes.MapPut("{id:guid}", Update)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageRecurrences));

        routes.MapPatch("{id:guid}/activate", Activate)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageRecurrences));

        routes.MapPatch("{id:guid}/deactivate", Deactivate)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageRecurrences));

        routes.MapDelete("{id:guid}", Delete)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageRecurrences));
    }

    private static async Task<IResult> GetAll(
        bool? active,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetExpenseRecurrencesQuery(active), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Create(
        CreateExpenseRecurrenceCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r =>
            TypedResults.Created($"/api/expense-recurrences/{r.Id}", r));
    }

    /// <summary>
    /// Runs generation now for the caller's store. Dispatches the very same command the scheduled
    /// worker uses, so a manual run can never behave differently from the nightly one (FR-039).
    /// </summary>
    private static async Task<IResult> Generate(
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GenerateRecurringExpensesCommand(), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateExpenseRecurrenceCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Activate(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SetExpenseRecurrenceActiveCommand(id, true), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Deactivate(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SetExpenseRecurrenceActiveCommand(id, false), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Delete(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteExpenseRecurrenceCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }
}
