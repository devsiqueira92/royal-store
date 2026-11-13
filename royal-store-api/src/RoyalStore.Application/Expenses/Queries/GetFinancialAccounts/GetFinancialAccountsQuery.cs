using MediatR;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetFinancialAccounts;

/// <summary>
/// Financial accounts. Supplying both <paramref name="OutflowFrom"/> and <paramref name="OutflowTo"/>
/// adds each account's outflow over that period — computed from payments, since no balance is
/// stored (FR-026a).
/// </summary>
public record GetFinancialAccountsQuery(
    bool IncludeInactive = false,
    DateOnly? OutflowFrom = null,
    DateOnly? OutflowTo = null) : IRequest<Result<IReadOnlyList<FinancialAccountResponse>>>;
