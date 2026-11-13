using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Expenses.Queries.GetFinancialAccounts;

public sealed class GetFinancialAccountsQueryHandler
    : IRequestHandler<GetFinancialAccountsQuery, Result<IReadOnlyList<FinancialAccountResponse>>>
{
    private readonly IFinancialAccountRepository _accountRepository;

    public GetFinancialAccountsQueryHandler(IFinancialAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result<IReadOnlyList<FinancialAccountResponse>>> Handle(
        GetFinancialAccountsQuery query, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetAllAsync(
            query.IncludeInactive ? null : true, cancellationToken);

        // Outflow is only computed when a complete period is asked for; otherwise the field stays
        // null rather than implying a balance the module does not keep.
        if (query.OutflowFrom is null || query.OutflowTo is null)
        {
            return Result<IReadOnlyList<FinancialAccountResponse>>.Success(
                [.. accounts.Select(a => FinancialAccountResponse.From(a))]);
        }

        if (query.OutflowTo.Value < query.OutflowFrom.Value)
            return Error.Validation("financialaccount.outflow_range_invalid",
                "The outflow period's end date cannot be earlier than its start date.");

        var outflow = await _accountRepository.GetOutflowByAccountAsync(
            query.OutflowFrom.Value, query.OutflowTo.Value, cancellationToken);

        return Result<IReadOnlyList<FinancialAccountResponse>>.Success(
            [.. accounts.Select(a => FinancialAccountResponse.From(
                a, outflow.TryGetValue(a.Id, out var total) ? total : 0m))]);
    }
}
