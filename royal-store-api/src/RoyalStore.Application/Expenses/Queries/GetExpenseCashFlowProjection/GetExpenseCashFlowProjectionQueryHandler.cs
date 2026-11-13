using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetExpenseCashFlowProjection;

public sealed class GetExpenseCashFlowProjectionQueryHandler
    : IRequestHandler<GetExpenseCashFlowProjectionQuery, Result<CashFlowProjectionResponse>>
{
    private const int MaxWeeks = 52;

    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseRecurrenceRepository _recurrenceRepository;

    public GetExpenseCashFlowProjectionQueryHandler(
        IExpenseRepository expenseRepository,
        IExpenseRecurrenceRepository recurrenceRepository)
    {
        _expenseRepository = expenseRepository;
        _recurrenceRepository = recurrenceRepository;
    }

    public async Task<Result<CashFlowProjectionResponse>> Handle(
        GetExpenseCashFlowProjectionQuery query, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekCount = query.Weeks is < 1 or > MaxWeeks ? 8 : query.Weeks;

        var horizonEnd = today.AddDays(7 * weekCount - 1);

        var weeks = Enumerable.Range(0, weekCount)
            .Select(i =>
            {
                var start = today.AddDays(7 * i);
                return (Start: start, End: start.AddDays(6));
            })
            .ToList();

        var unsettled = await _expenseRepository.GetUnsettledAsync(cancellationToken);
        var templates = await _recurrenceRepository.GetActiveAsync(cancellationToken);

        var overdueTotal = 0m;
        var committedByWeek = new decimal[weekCount];
        var projectedByWeek = new decimal[weekCount];

        // Bills that already exist. Each contributes what is still OWED, not its full amount, so a
        // partly settled bill does not overstate the money that still has to leave.
        foreach (var expense in unsettled)
        {
            var outstanding = expense.RemainingBalance;
            if (outstanding <= 0)
                continue;

            if (expense.DueDate < today)
            {
                overdueTotal += outstanding;
                continue;
            }

            var weekIndex = WeekIndexOf(expense.DueDate, today, weekCount);
            if (weekIndex is not null)
                committedByWeek[weekIndex.Value] += outstanding;
        }

        // Which template cycles already have a bill. Keyed by the SAME cycle key the generator
        // writes, so this test and idempotency can never disagree — matching on date ranges instead
        // would double-count a bill whose due date was edited by hand (research Decision 7).
        var committedCycleKeys = unsettled
            .Where(e => e.ExpenseRecurrenceId is not null && e.RecurrenceCycleKey is not null)
            .Select(e => (RecurrenceId: e.ExpenseRecurrenceId!.Value, CycleKey: e.RecurrenceCycleKey!))
            .ToHashSet();

        foreach (var template in templates)
        {
            if (template.IsFinished(today))
                continue;

            // EnumerateDueCycles stops at the creation window; the projection needs cycles further
            // out, so it is asked for everything up to the horizon's end.
            foreach (var dueDate in template.EnumerateDueCycles(horizonEnd))
            {
                if (dueDate < today || dueDate > horizonEnd)
                    continue;

                // Already created: it is counted under committed above, never again here (FR-046).
                if (committedCycleKeys.Contains((template.Id, template.CycleKeyFor(dueDate))))
                    continue;

                var weekIndex = WeekIndexOf(dueDate, today, weekCount);
                if (weekIndex is not null)
                    projectedByWeek[weekIndex.Value] += template.Amount;
            }
        }

        var weekRows = weeks
            .Select((w, i) => new CashFlowWeek(
                w.Start,
                w.End,
                // Overdue is money already past due, so it belongs to no future week; it is
                // reported once, against the first bucket and the headline figure.
                i == 0 ? overdueTotal : 0m,
                committedByWeek[i],
                projectedByWeek[i]))
            .ToList();

        var next30Cutoff = today.AddDays(30);

        var next30Committed = committedByWeek
            .Where((_, i) => weeks[i].Start <= next30Cutoff)
            .Sum();

        var next30Projected = projectedByWeek
            .Where((_, i) => weeks[i].Start <= next30Cutoff)
            .Sum();

        var horizonCommitted = committedByWeek.Sum();
        var horizonProjected = projectedByWeek.Sum();

        return new CashFlowProjectionResponse(
            overdueTotal,
            new CashFlowBucket(next30Committed + next30Projected, next30Committed, next30Projected),
            new CashFlowBucket(horizonCommitted + horizonProjected, horizonCommitted, horizonProjected),
            weekRows);
    }

    private static int? WeekIndexOf(DateOnly date, DateOnly today, int weekCount)
    {
        var dayOffset = date.DayNumber - today.DayNumber;
        if (dayOffset < 0)
            return null;

        var index = dayOffset / 7;
        return index < weekCount ? index : null;
    }
}
