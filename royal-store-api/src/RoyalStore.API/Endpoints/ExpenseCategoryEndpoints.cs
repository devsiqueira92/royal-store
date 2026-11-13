using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Expenses.Commands.CreateExpenseCategory;
using RoyalStore.Application.Expenses.Commands.DeleteExpenseCategory;
using RoyalStore.Application.Expenses.Commands.SetExpenseCategoryActive;
using RoyalStore.Application.Expenses.Commands.UpdateExpenseCategory;
using RoyalStore.Application.Expenses.Queries.GetExpenseCategories;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.API.Endpoints;

/// <summary>The hierarchical expense categories the management P&amp;L rolls up.</summary>
public sealed class ExpenseCategoryEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/expense-categories").WithTags("ExpenseCategories");

        // Reading categories is part of recording an expense, so it needs only ExpensesView.
        routes.MapGet("", GetAll)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesView));

        routes.MapPost("", Create)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        routes.MapPut("{id:guid}", Update)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        routes.MapPatch("{id:guid}/activate", Activate)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        routes.MapPatch("{id:guid}/deactivate", Deactivate)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        routes.MapDelete("{id:guid}", Delete)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));
    }

    private static async Task<IResult> GetAll(
        [FromQuery(Name = "include-inactive")] bool? includeInactive,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetExpenseCategoriesQuery(includeInactive ?? false), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Create(
        CreateExpenseCategoryCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r =>
            TypedResults.Created($"/api/expense-categories/{r.Id}", r));
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateExpenseCategoryCommand command,
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
        var result = await mediator.Send(new SetExpenseCategoryActiveCommand(id, true), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Deactivate(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new SetExpenseCategoryActiveCommand(id, false), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Delete(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteExpenseCategoryCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }
}
