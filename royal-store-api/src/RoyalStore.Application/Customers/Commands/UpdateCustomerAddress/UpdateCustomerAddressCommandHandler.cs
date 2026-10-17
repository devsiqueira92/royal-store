using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Customers;

namespace RoyalStore.Application.Customers.Commands.UpdateCustomerAddress;

public sealed class UpdateCustomerAddressCommandHandler
    : IRequestHandler<UpdateCustomerAddressCommand, Result<CustomerAddressResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCustomerAddressCommandHandler(
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
        UpdateCustomerAddressCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdWithAddressesAsync(command.CustomerId, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

        var postal = PostalAddress.Create(command.ZipCode, command.Street, command.Number, command.Neighborhood,
            command.City, command.State, command.Complement, command.AdditionalInfo);
        if (postal.IsFailure)
            return postal.Error;

        var updateResult = customer.UpdateAddress(command.AddressId, postal.Value, command.IsDefaultDelivery);
        if (updateResult.IsFailure)
            return updateResult.Error;

        var address = updateResult.Value;

        try
        {
            // Tracked customer — change detection picks up the edits; no UpdateAsync(customer).
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Error.Conflict("customer.concurrent_update",
                "The customer was changed by another operation. Refresh and try again.");
        }

        await _auditService.RecordAsync(
            "customer.address_updated",
            _tenantContext.TenantId,
            $"Address {address.Id} of customer {customer.Id} updated.",
            _currentUserService.CurrentUserId ?? "unknown",
            cancellationToken);

        return CustomerAddressResponse.From(address);
    }
}
