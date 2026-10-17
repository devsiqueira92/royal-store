using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly ICashbackLedger _cashbackLedger;
    private readonly ITenantContext _tenantContext;

    public GetCustomerByIdQueryHandler(
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

    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Returned regardless of IsActive — deactivation removes a customer from listings and
        // checkout selection, not from direct lookup (contracts/customers-api.md).
        var customer = await _customerRepository.GetByIdWithAddressesAsync(request.Id, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

        var tier = await _loyaltyTierRepository.GetByIdAsync(customer.LoyaltyTierId, cancellationToken);
        if (tier is null)
            return Error.NotFound("customer.tier_not_found", "Customer's loyalty tier not found.");

        var balance = await _cashbackLedger.GetUsableBalanceAsync(_tenantContext.TenantId, customer.Id, cancellationToken);

        // Every address, active and inactive (045 FR-013): default first, then active, then oldest.
        var addresses = customer.Addresses
            .OrderByDescending(a => a.IsDefaultDelivery)
            .ThenByDescending(a => a.IsActive)
            .ThenBy(a => a.CreatedAt)
            .Select(CustomerAddressResponse.From)
            .ToList();

        return CustomerResponse.From(customer, tier, balance, addresses);
    }
}
