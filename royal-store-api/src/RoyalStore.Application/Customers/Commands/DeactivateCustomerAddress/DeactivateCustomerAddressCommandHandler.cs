using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.DeactivateCustomerAddress;

public sealed class DeactivateCustomerAddressCommandHandler
    : IRequestHandler<DeactivateCustomerAddressCommand, Result<CustomerAddressResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateCustomerAddressCommandHandler(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CustomerAddressResponse>> Handle(
        DeactivateCustomerAddressCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdWithAddressesAsync(command.CustomerId, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

        var deactivateResult = customer.DeactivateAddress(command.AddressId);
        if (deactivateResult.IsFailure)
            return deactivateResult.Error;

        var address = deactivateResult.Value;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict("customer.concurrent_update",
                "The customer was changed by another operation. Refresh and try again.");
        }

        await _auditService.RecordAsync(
            "customer.address_deactivated",
            _tenantContext.TenantId,
            $"Address {address.Id} of customer {customer.Id} deactivated.",
            _currentUserService.CurrentUserId ?? "unknown",
            cancellationToken);

        return CustomerAddressResponse.From(address);
    }
}
