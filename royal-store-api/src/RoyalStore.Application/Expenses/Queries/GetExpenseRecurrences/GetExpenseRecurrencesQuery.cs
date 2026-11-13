using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetExpenseRecurrences;

/// <summary>Every template with its next bill date and the store's monthly commitment (FR-036).</summary>
public record GetExpenseRecurrencesQuery(bool? OnlyActive = null)
    : IRequest<Result<ExpenseRecurrenceListResponse>>;
