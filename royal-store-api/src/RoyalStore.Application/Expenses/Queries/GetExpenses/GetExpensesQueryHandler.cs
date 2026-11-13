using System.Globalization;
using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Queries.GetExpenses;

public sealed class GetExpensesQueryHandler
    : IRequestHandler<GetExpensesQuery, Result<ExpenseMonthResponse>>
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;

    public GetExpensesQueryHandler(
        IExpenseRepository expenseRepository,
        IExpenseCategoryRepository categoryRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ExpenseMonthResponse>> Handle(
        GetExpensesQuery query, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var month = query.CompetenceMonth is null
            ? new DateOnly(today.Year, today.Month, 1)
            : new DateOnly(query.CompetenceMonth.Value.Year, query.CompetenceMonth.Value.Month, 1);

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 200 ? 50 : query.PageSize;

        // The whole month, unfiltered: the summary must answer "what do I owe this month?" and so
        // cannot move when the user changes a filter tab.
        var monthExpenses = await _expenseRepository.GetByCompetenceMonthAsync(
            new ExpenseListFilter(month), cancellationToken);

        var summary = BuildSummary(monthExpenses, today);

        // Filters narrow only the list. Status and category are already applied in the database;
        // overdue is applied here because it is derived from the due date and settled total.
        var filtered = monthExpenses.AsEnumerable();

        if (query.Status.HasValue)
            filtered = filtered.Where(e => e.Status == query.Status.Value);

        if (query.CategoryId.HasValue)
            filtered = filtered.Where(e => e.ExpenseCategoryId == query.CategoryId.Value);

        if (query.Overdue == true)
            filtered = filtered.Where(e => e.IsOverdue(today));

        var filteredList = filtered.ToList();

        var categories = await _categoryRepository.GetAllAsync(ct: cancellationToken);
        var categoryById = categories.ToDictionary(c => c.Id);

        var items = filteredList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => ExpenseListItemResponse.From(e, ResolveCategory(categoryById, e), today))
            .ToList();

        return new ExpenseMonthResponse(
            month.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            summary,
            items,
            filteredList.Sum(e => e.Amount),
            page,
            pageSize,
            filteredList.Count);
    }

    private static ExpenseCategorySummary ResolveCategory(
        IReadOnlyDictionary<Guid, ExpenseCategory> categoryById, Expense expense) =>
        categoryById.TryGetValue(expense.ExpenseCategoryId, out var category)
            ? ExpenseCategorySummary.From(category)
            : new ExpenseCategorySummary(expense.ExpenseCategoryId, string.Empty, null);

    /// <summary>
    /// The four figures (FR-041). Every total is computed in memory over rows already loaded, so
    /// nothing here depends on a projection translating (Constitution VI).
    /// </summary>
    private static ExpenseMonthSummaryResponse BuildSummary(
        IReadOnlyList<Expense> expenses, DateOnly today)
    {
        var total = expenses.Sum(e => e.Amount);
        var paid = expenses.Sum(e => e.SettledAmount);

        // Overdue counts what is still OWED on a past-due bill, not its full amount (FR-021): a
        // bill of 2,180 with 1,200 settled is 980 overdue.
        var overdueExpenses = expenses.Where(e => e.IsOverdue(today)).ToList();
        var overdue = overdueExpenses.Sum(e => e.RemainingBalance);

        // Pending is what is not yet due and not yet settled — the complement of overdue, so the
        // two never double-count the same money.
        var pendingExpenses = expenses
            .Where(e => !e.IsOverdue(today) && e.Status != ExpenseStatus.Paid)
            .ToList();

        var pending = pendingExpenses.Sum(e => e.RemainingBalance);

        var paidPercentage = total == 0 ? 0 : (int)Math.Round(paid / total * 100, MidpointRounding.AwayFromZero);

        return new ExpenseMonthSummaryResponse(
            total,
            expenses.Count,
            paid,
            paidPercentage,
            pending,
            pendingExpenses.Count,
            overdue,
            overdueExpenses.Count);
    }
}
