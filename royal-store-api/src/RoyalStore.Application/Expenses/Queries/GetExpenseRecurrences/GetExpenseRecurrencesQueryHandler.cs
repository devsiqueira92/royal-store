using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Queries.GetExpenseRecurrences;

public sealed class GetExpenseRecurrencesQueryHandler
    : IRequestHandler<GetExpenseRecurrencesQuery, Result<ExpenseRecurrenceListResponse>>
{
    private readonly IExpenseRecurrenceRepository _recurrenceRepository;
    private readonly IExpenseCategoryRepository _categoryRepository;

    public GetExpenseRecurrencesQueryHandler(
        IExpenseRecurrenceRepository recurrenceRepository,
        IExpenseCategoryRepository categoryRepository)
    {
        _recurrenceRepository = recurrenceRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<ExpenseRecurrenceListResponse>> Handle(
        GetExpenseRecurrencesQuery query, CancellationToken cancellationToken)
    {
        var all = await _recurrenceRepository.GetAllAsync(cancellationToken);

        var categories = await _categoryRepository.GetAllAsync(ct: cancellationToken);
        var categoryById = categories.ToDictionary(c => c.Id);

        var visible = query.OnlyActive == true
            ? all.Where(r => r.IsActive).ToList()
            : all.ToList();

        var items = visible
            .Select(r => ExpenseRecurrenceResponse.From(
                r,
                categoryById.TryGetValue(r.ExpenseCategoryId, out var category)
                    ? ExpenseCategorySummary.From(category)
                    : new ExpenseCategorySummary(r.ExpenseCategoryId, string.Empty, null)))
            .ToList();

        return new ExpenseRecurrenceListResponse(
            items,
            all.Count(r => r.IsActive),
            all.Count,
            all.Where(r => r.IsActive).Sum(MonthlyEquivalent));
    }

    /// <summary>
    /// Normalises a template's amount to a monthly figure so the total means "what this store is
    /// committed to per month" — a quarterly bill contributes a third of itself, not all of it.
    /// </summary>
    private static decimal MonthlyEquivalent(ExpenseRecurrence recurrence) => recurrence.Frequency switch
    {
        // 52 weeks over 12 months.
        RecurrenceFrequency.Weekly => recurrence.Amount * 52m / 12m,
        RecurrenceFrequency.Monthly => recurrence.Amount,
        RecurrenceFrequency.Quarterly => recurrence.Amount / 3m,
        RecurrenceFrequency.Semiannually => recurrence.Amount / 6m,
        RecurrenceFrequency.Yearly => recurrence.Amount / 12m,
        _ => recurrence.Amount,
    };
}
