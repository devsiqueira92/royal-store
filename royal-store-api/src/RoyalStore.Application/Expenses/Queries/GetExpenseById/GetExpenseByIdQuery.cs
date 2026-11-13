using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetExpenseById;

/// <summary>One expense in full, with its payments and attachments (FR-017, FR-042, FR-049).</summary>
public record GetExpenseByIdQuery(Guid Id) : IRequest<Result<ExpenseResponse>>;
