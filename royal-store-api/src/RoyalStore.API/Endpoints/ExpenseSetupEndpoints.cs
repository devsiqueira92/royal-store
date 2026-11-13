using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Expenses.Commands.ManageCostCenters;
using RoyalStore.Application.Expenses.Commands.ManageFinancialAccounts;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Application.Expenses.Queries.GetCostCenters;
using RoyalStore.Application.Expenses.Queries.GetFinancialAccounts;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.API.Endpoints;

/// <summary>
/// Cost centers and financial accounts — the two supporting classifications of the OPEX module.
/// </summary>
public sealed class ExpenseSetupEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var costCenters = app.MapGroup("api/cost-centers").WithTags("ExpenseSetup");

        costCenters.MapGet("", GetCostCenters)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesView));

        costCenters.MapPost("", CreateCostCenter)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        costCenters.MapPut("{id:guid}", UpdateCostCenter)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        costCenters.MapPatch("{id:guid}/activate", ActivateCostCenter)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        costCenters.MapPatch("{id:guid}/deactivate", DeactivateCostCenter)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        costCenters.MapDelete("{id:guid}", DeleteCostCenter)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        var accounts = app.MapGroup("api/financial-accounts").WithTags("ExpenseSetup");

        accounts.MapGet("", GetFinancialAccounts)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesView));

        accounts.MapPost("", CreateFinancialAccount)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        accounts.MapPut("{id:guid}", UpdateFinancialAccount)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        accounts.MapPatch("{id:guid}/activate", ActivateFinancialAccount)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        accounts.MapPatch("{id:guid}/deactivate", DeactivateFinancialAccount)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));

        accounts.MapDelete("{id:guid}", DeleteFinancialAccount)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesManageCategories));
    }

    // ------------------------------------------------------------------ cost centers

    private static async Task<IResult> GetCostCenters(
        [FromQuery(Name = "include-inactive")] bool? includeInactive,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetCostCentersQuery(includeInactive ?? false), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> CreateCostCenter(
        CreateCostCenterCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/cost-centers/{r.Id}", r));
    }

    private static async Task<IResult> UpdateCostCenter(
        Guid id,
        UpdateCostCenterCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> ActivateCostCenter(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetCostCenterActiveCommand(id, true), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> DeactivateCostCenter(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetCostCenterActiveCommand(id, false), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> DeleteCostCenter(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteCostCenterCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    // ------------------------------------------------------------- financial accounts

    private static async Task<IResult> GetFinancialAccounts(
        [FromQuery(Name = "include-inactive")] bool? includeInactive,
        [FromQuery(Name = "outflow-from")] DateOnly? outflowFrom,
        [FromQuery(Name = "outflow-to")] DateOnly? outflowTo,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetFinancialAccountsQuery(includeInactive ?? false, outflowFrom, outflowTo), ct);

        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> CreateFinancialAccount(
        CreateFinancialAccountRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var type = FinancialAccountResponse.ParseType(request.Type);
        if (type is null)
            return InvalidAccountType();

        var result = await mediator.Send(
            new CreateFinancialAccountCommand(request.Name, type.Value), ct);

        return result.ToHttpResult(r => TypedResults.Created($"/api/financial-accounts/{r.Id}", r));
    }

    private static async Task<IResult> UpdateFinancialAccount(
        Guid id,
        CreateFinancialAccountRequest request,
        IMediator mediator,
        CancellationToken ct)
    {
        var type = FinancialAccountResponse.ParseType(request.Type);
        if (type is null)
            return InvalidAccountType();

        var result = await mediator.Send(
            new UpdateFinancialAccountCommand(id, request.Name, type.Value), ct);

        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> ActivateFinancialAccount(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetFinancialAccountActiveCommand(id, true), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> DeactivateFinancialAccount(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new SetFinancialAccountActiveCommand(id, false), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> DeleteFinancialAccount(
        Guid id, IMediator mediator, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteFinancialAccountCommand(id), ct);
        return result.ToHttpResult(() => TypedResults.NoContent());
    }

    private static IResult InvalidAccountType() =>
        Results.BadRequest(new
        {
            Code = "financialaccount.type_invalid",
            Message = "Type must be one of: cash, bank-account, digital-wallet.",
        });

    /// <summary>
    /// The account type arrives as the same lowercase-hyphen string the responses use, rather than
    /// a raw enum name, so the API reads consistently in both directions.
    /// </summary>
    public sealed record CreateFinancialAccountRequest(string Name, string Type);
}
