using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Customers;

namespace RoyalStore.Application.Customers.Commands.AddCustomerAddress;

public sealed class AddCustomerAddressCommandHandler
    : IRequestHandler<AddCustomerAddressCommand, Result<CustomerAddressResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public AddCustomerAddressCommandHandler(
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
        AddCustomerAddressCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdWithAddressesAsync(command.CustomerId, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

        var postal = PostalAddress.Create(command.ZipCode, command.Street, command.Number, command.Neighborhood,
            command.City, command.State, command.Complement, command.AdditionalInfo);
        if (postal.IsFailure)
            return postal.Error;

        var addResult = customer.AddAddress(Guid.NewGuid(), postal.Value, command.IsDefaultDelivery);
        if (addResult.IsFailure)
            return addResult.Error;

        var address = addResult.Value;

        // No UpdateAsync(customer): it's tracked, and Update() would mark the new address Modified.
        await _customerRepository.AddAddressAsync(address, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another address change on this customer committed first (Customer.Version); nothing is
            // saved, so the one-default rule still holds and the caller can simply retry.
            return Error.Conflict("customer.concurrent_update",
                "The customer was changed by another operation. Refresh and try again.");
        }

        await _auditService.RecordAsync(
            "customer.address_added",
            _tenantContext.TenantId,
            $"Address {address.Id} added to customer {customer.Id}.",
            _currentUserService.CurrentUserId ?? "unknown",
            cancellationToken);

        return CustomerAddressResponse.From(address);
    }
}
