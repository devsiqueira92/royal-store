using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Abstractions.Data;

/// <summary>
/// Filters for the competence-month expense list (FR-043). Overdue is a separate flag rather than a
/// status value because it is derived from the due date, not stored (FR-020) — an expense can be
/// partially paid and overdue at once.
/// </summary>
public sealed record ExpenseListFilter(
    DateOnly CompetenceMonth,
    ExpenseStatus? Status = null,
    bool? OnlyOverdue = null,
    Guid? CategoryId = null);
