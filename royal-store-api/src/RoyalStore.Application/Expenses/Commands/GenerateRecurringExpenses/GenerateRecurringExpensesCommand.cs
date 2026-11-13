using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Commands.GenerateRecurringExpenses;

/// <summary>
/// Creates the bills whose creation window has opened for the CURRENT tenant (FR-031).
/// </summary>
/// <remarks>
/// One command serves both callers — the daily worker and the manual trigger endpoint — so the two
/// can never drift apart. The tenant comes from ITenantContext either way: from the JWT for the
/// manual trigger, from the worker's ambient TenantScope for the scheduled run.
/// </remarks>
public record GenerateRecurringExpensesCommand(DateOnly? Today = null)
    : IRequest<Result<RecurrenceGenerationResultResponse>>;
