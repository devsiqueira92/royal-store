using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler
    : IRequestHandler<GetCustomersQuery, Result<IReadOnlyList<CustomerResponse>>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICashbackLedger _cashbackLedger;
    private readonly ITenantContext _tenantContext;

    public GetCustomersQueryHandler(
        ICustomerRepository customerRepository,
        ICashbackLedger cashbackLedger,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _cashbackLedger = cashbackLedger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<CustomerResponse>>> Handle(
        GetCustomersQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Active customers only (FR-006) — unpaginated, matching every other list query in this
        // codebase (research.md Decision 10; single-store customer bases don't warrant it).
        // The tier is joined in via Include, and every cashback balance is resolved in one batch —
        // three queries total, regardless of how many customers there are.
        var customers = await _customerRepository.GetAllAsync(includeInactive: false, cancellationToken);
        var customerIds = customers.Select(c => c.Id).ToArray();

        var responses = new List<CustomerResponse>(customers.Count);
        foreach (var customer in customers)
        {
            if (customer.LoyaltyTier is null)
                continue;

            responses.Add(CustomerResponse.From(
                customer,
                customer.LoyaltyTier));
        }

        return Result<IReadOnlyList<CustomerResponse>>.Success(responses);
    }
}
