using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.GetCustomerCashbackBalance;

public sealed class GetCustomerCashbackBalanceQueryHandler
    : IRequestHandler<GetCustomerCashbackBalanceQuery, Result<decimal>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICashbackLedger _cashbackLedger;
    private readonly ITenantContext _tenantContext;

    public GetCustomerCashbackBalanceQueryHandler(
        ICustomerRepository customerRepository,
        ICashbackLedger cashbackLedger,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _cashbackLedger = cashbackLedger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<decimal>> Handle(
        GetCustomerCashbackBalanceQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

        var usable = await _cashbackLedger.GetUsableBalanceAsync(
            _tenantContext.TenantId, customer.Id, cancellationToken);

        // Only what can actually be spent: an outstanding clawback can drive the raw balance
        // negative, but nothing is available for consumption in that state.
        return Math.Max(0m, usable);
    }
}
