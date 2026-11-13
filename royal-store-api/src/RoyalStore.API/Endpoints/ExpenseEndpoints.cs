using System.Globalization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RoyalStore.API.Extensions;
using RoyalStore.Application.Expenses.Commands.CancelExpense;
using RoyalStore.Application.Expenses.Commands.CreateExpense;
using RoyalStore.Application.Expenses.Commands.RegisterExpensePayment;
using RoyalStore.Application.Expenses.Commands.ReverseExpensePayment;
using RoyalStore.Application.Expenses.Commands.UpdateExpense;
using RoyalStore.Application.Expenses.Queries.GetExpenseById;
using RoyalStore.Application.Expenses.Queries.GetExpenses;
using RoyalStore.Application.Expenses.Queries.GetExpenseCashFlowProjection;
using RoyalStore.Application.Expenses.Queries.GetExpensesByCategoryReport;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Expenses;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.API.Endpoints;

/// <summary>
/// Operating expenses (OPEX) — what it costs to keep the doors open. Every route is gated by its
/// own permission so access can be granted per user rather than by role alone (FR-051).
/// </summary>
public sealed class ExpenseEndpoints : IEndpointDefinition
{
    public void RegisterEndpoints(WebApplication app)
    {
        var routes = app.MapGroup("api/expenses").WithTags("Expenses");

        routes.MapPost("", Create)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesCreate));

        routes.MapGet("", GetMonth)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesView));

        // Reports sit behind their own permission so day-to-day payable work can be granted
        // without exposing the management P&L, which is the sensitive view (FR-051).
        routes.MapGet("reports/by-category", GetByCategoryReport)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesViewReports));

        routes.MapGet("reports/cash-flow", GetCashFlowProjection)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesViewReports));

        routes.MapGet("{id:guid}", GetById)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesView));

        routes.MapPut("{id:guid}", Update)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesCreate));

        routes.MapPatch("{id:guid}/cancel", Cancel)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesCancel));

        routes.MapPost("{expenseId:guid}/payments", RegisterPayment)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesRegisterPayment));

        routes.MapPatch("{expenseId:guid}/payments/{paymentId:guid}/reverse", ReversePayment)
            .RequireAuthorization(PolicyNames.Permission(Permission.ExpensesReversePayment));
    }

    private static async Task<IResult> Create(
        CreateExpenseCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return result.ToHttpResult(r => TypedResults.Created($"/api/expenses/{r.Id}", r));
    }

    private static async Task<IResult> GetMonth(
        [FromQuery(Name = "competence-month")] string? competenceMonth,
        [FromQuery(Name = "status")] string? status,
        [FromQuery(Name = "overdue")] bool? overdue,
        [FromQuery(Name = "category-id")] Guid? categoryId,
        [FromQuery(Name = "page")] int? page,
        [FromQuery(Name = "page-size")] int? pageSize,
        IMediator mediator,
        CancellationToken ct)
    {
        DateOnly? month = null;
        if (!string.IsNullOrWhiteSpace(competenceMonth))
        {
            // Accepts "2026-11"; anything else is a client error rather than a silent fallback to
            // the current month, which would quietly answer a different question than was asked.
            if (!DateOnly.TryParseExact(competenceMonth + "-01", "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return Results.BadRequest(new
                {
                    Code = "expense.competence_month_invalid",
                    Message = "Competence month must be in yyyy-MM format.",
                });
            }

            month = parsed;
        }

        ExpenseStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status))
        {
            parsedStatus = status.Trim().ToLowerInvariant() switch
            {
                "pending" => ExpenseStatus.Pending,
                "partially-paid" => ExpenseStatus.PartiallyPaid,
                "paid" => ExpenseStatus.Paid,
                "cancelled" => ExpenseStatus.Cancelled,
                _ => null,
            };

            if (parsedStatus is null)
            {
                return Results.BadRequest(new
                {
                    Code = "expense.status_invalid",
                    Message = "Status must be one of: pending, partially-paid, paid, cancelled.",
                });
            }
        }

        var result = await mediator.Send(
            new GetExpensesQuery(month, parsedStatus, overdue, categoryId, page ?? 1, pageSize ?? 50), ct);

        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetByCategoryReport(
        [FromQuery(Name = "competence-month")] string? competenceMonth,
        [FromQuery(Name = "months")] int? months,
        IMediator mediator,
        CancellationToken ct)
    {
        DateOnly? month = null;
        if (!string.IsNullOrWhiteSpace(competenceMonth))
        {
            if (!DateOnly.TryParseExact(competenceMonth + "-01", "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
            {
                return Results.BadRequest(new
                {
                    Code = "expense.competence_month_invalid",
                    Message = "Competence month must be in yyyy-MM format.",
                });
            }

            month = parsed;
        }

        var result = await mediator.Send(
            new GetExpensesByCategoryReportQuery(month, months ?? 6), ct);

        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetCashFlowProjection(
        [FromQuery(Name = "weeks")] int? weeks,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetExpenseCashFlowProjectionQuery(weeks ?? 8), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> GetById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new GetExpenseByIdQuery(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateExpenseCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        // The route id wins over the body, so a mismatched payload cannot edit another expense.
        var result = await mediator.Send(command with { Id = id }, ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> Cancel(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(new CancelExpenseCommand(id), ct);
        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    private static async Task<IResult> RegisterPayment(
        Guid expenseId,
        RegisterExpensePaymentCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command with { ExpenseId = expenseId }, ct);
        return result.ToHttpResult(r =>
            TypedResults.Created($"/api/expenses/{r.Id}", r));
    }

    private static async Task<IResult> ReversePayment(
        Guid expenseId,
        Guid paymentId,
        ReverseExpensePaymentRequest? request,
        IMediator mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(
            new ReverseExpensePaymentCommand(expenseId, paymentId, request?.Reason), ct);

        return result.ToHttpResult(r => TypedResults.Ok(r));
    }

    /// <summary>Body of the reversal request; the ids come from the route.</summary>
    public sealed record ReverseExpensePaymentRequest(string? Reason);
}
