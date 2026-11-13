using System.Globalization;
using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Queries.GetExpensesByCategoryReport;

public sealed class GetExpensesByCategoryReportQueryHandler
    : IRequestHandler<GetExpensesByCategoryReportQuery, Result<ExpensesByCategoryReportResponse>>
{
    private const int MaxMonths = 36;

    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;

    public GetExpensesByCategoryReportQueryHandler(
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository categoryRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ExpensesByCategoryReportResponse>> Handle(
        GetExpensesByCategoryReportQuery query, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var lastMonth = query.CompetenceMonth is null
            ? new DateOnly(today.Year, today.Month, 1)
            : new DateOnly(query.CompetenceMonth.Value.Year, query.CompetenceMonth.Value.Month, 1);

        var monthCount = query.Months is < 1 or > MaxMonths ? 6 : query.Months;

        var firstMonth = lastMonth.AddMonths(-(monthCount - 1));
        var exclusiveEnd = lastMonth.AddMonths(1);

        var months = Enumerable.Range(0, monthCount)
            .Select(i => firstMonth.AddMonths(i))
            .ToList();

        var monthIndex = months
            .Select((m, i) => (Month: m, Index: i))
            .ToDictionary(x => x.Month, x => x.Index);

        // Two flat reads, then all shaping in memory: no GroupBy projection is sent to the
        // provider, so nothing here can fail only at runtime (Constitution VI, research D8).
        var categories = await _categoryRepository.GetAllAsync(ct: cancellationToken);
        var expenses = await _expenseRepository.GetByCompetenceRangeAsync(
            firstMonth, exclusiveEnd, cancellationToken);

        // A parent reports the sum of its own expenses and all its descendants' (FR-023), so every
        // expense is attributed to its top-level ancestor. The walk is in memory because a store
        // has tens of categories and a recursive CTE cannot be written in LINQ (research D5).
        var categoryById = categories.ToDictionary(c => c.Id);
        var rootOf = new Dictionary<Guid, Guid>();

        foreach (var category in categories)
            rootOf[category.Id] = FindRoot(category, categoryById);

        var rootCategories = categories
            .Where(c => c.ParentId is null)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToList();

        // Null until an expense lands in that cell, so "no expense" stays distinguishable from zero.
        var values = rootCategories.ToDictionary(c => c.Id, _ => new decimal?[monthCount]);
        var totals = new decimal[monthCount];

        foreach (var expense in expenses)
        {
            if (!monthIndex.TryGetValue(expense.CompetenceMonth, out var index))
                continue;

            if (!rootOf.TryGetValue(expense.ExpenseCategoryId, out var rootId) ||
                !values.TryGetValue(rootId, out var row))
            {
                continue;
            }

            row[index] = (row[index] ?? 0m) + expense.Amount;
            totals[index] += expense.Amount;
        }

        var lastIndex = monthCount - 1;
        var lastMonthTotal = totals[lastIndex];

        var rows = rootCategories
            .Select(c =>
            {
                var row = values[c.Id];
                var lastValue = row[lastIndex] ?? 0m;

                var share = lastMonthTotal == 0
                    ? 0m
                    : Math.Round(lastValue / lastMonthTotal * 100m, 1, MidpointRounding.AwayFromZero);

                return new ExpenseCategoryReportRow(c.Id, c.Name, c.Color, row, share);
            })
            .ToList();

        return new ExpensesByCategoryReportResponse(
            [.. months.Select(m => m.ToString("yyyy-MM", CultureInfo.InvariantCulture))],
            rows,
            totals);
    }

    /// <summary>
    /// Walks to the top-level ancestor, guarding against a cycle the data should not contain but
    /// which must not hang the report if it does.
    /// </summary>
    private static Guid FindRoot(
        ExpenseCategory category, IReadOnlyDictionary<Guid, ExpenseCategory> categoryById)
    {
        var current = category;
        var guard = 0;

        while (current.ParentId is not null &&
               categoryById.TryGetValue(current.ParentId.Value, out var parent))
        {
            current = parent;

            if (++guard > categoryById.Count)
                break;
        }

        return current.Id;
    }
}
