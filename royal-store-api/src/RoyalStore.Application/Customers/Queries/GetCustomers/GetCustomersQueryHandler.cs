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
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly ICashbackLedger _cashbackLedger;
    private readonly ITenantContext _tenantContext;

    public GetCustomersQueryHandler(
        ICustomerRepository customerRepository,
        ILoyaltyTierRepository loyaltyTierRepository,
        ICashbackLedger cashbackLedger,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _loyaltyTierRepository = loyaltyTierRepository;
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
        var customers = await _customerRepository.GetAllAsync(includeInactive: false, cancellationToken);
        var tiers = (await _loyaltyTierRepository.GetAllAsync(cancellationToken)).ToDictionary(t => t.Id);

        var responses = new List<CustomerResponse>(customers.Count);
        foreach (var customer in customers)
        {
            if (!tiers.TryGetValue(customer.LoyaltyTierId, out var tier))
                continue;

            var balance = await _cashbackLedger.GetUsableBalanceAsync(tenantId, customer.Id, cancellationToken);
            responses.Add(CustomerResponse.From(customer, tier, balance));
        }

        return Result<IReadOnlyList<CustomerResponse>>.Success(responses);
    }
}
