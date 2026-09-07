using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Queries.GetCustomerCashbackHistory;

public sealed class GetCustomerCashbackHistoryQueryHandler
    : IRequestHandler<GetCustomerCashbackHistoryQuery, Result<IReadOnlyList<CashbackEntryResponse>>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICashbackLedger _cashbackLedger;
    private readonly ITenantContext _tenantContext;

    public GetCustomerCashbackHistoryQueryHandler(
        ICustomerRepository customerRepository,
        ICashbackLedger cashbackLedger,
        ITenantContext tenantContext)
    {
        _customerRepository = customerRepository;
        _cashbackLedger = cashbackLedger;
        _tenantContext = tenantContext;
    }

    public async Task<Result<IReadOnlyList<CashbackEntryResponse>>> Handle(
        GetCustomerCashbackHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

        var now = DateTime.UtcNow;
        var entries = await _cashbackLedger.GetHistoryAsync(_tenantContext.TenantId, customer.Id, cancellationToken);
        var responses = entries.Select(e => CashbackEntryResponse.From(e, now)).ToList();

        return Result<IReadOnlyList<CashbackEntryResponse>>.Success(responses);
    }
}
