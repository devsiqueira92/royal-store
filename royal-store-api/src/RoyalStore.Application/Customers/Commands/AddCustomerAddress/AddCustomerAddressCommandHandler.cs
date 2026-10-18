using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.AddCustomerAddress;

public sealed class AddCustomerAddressCommandHandler
    : IRequestHandler<AddCustomerAddressCommand, Result<IReadOnlyList<CustomerAddressResponse>>>
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

    public async Task<Result<IReadOnlyList<CustomerAddressResponse>>> Handle(
        AddCustomerAddressCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdWithAddressesAsync(command.CustomerId, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

        var addressChanges = CustomerAddressChanges.From(command.Addresses);
        if (addressChanges.IsFailure)
            return addressChanges.Error;

        // Blank entries are kept on add (045 FR-011a, 046 FR-023).
        var applied = customer.ApplyAddressChanges(addressChanges.Value, keepEmptyNewAddresses: true);
        if (applied.IsFailure)
            return applied.Error;

        var added = applied.Value.Added;

        // No UpdateAsync(customer): it's tracked, and Update() would mark the new addresses Modified.
        foreach (var address in added)
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

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        foreach (var address in added)
            await _auditService.RecordAsync(
                "customer.address_added",
                _tenantContext.TenantId,
                $"Address {address.Id} added to customer {customer.Id}.",
                userId,
                cancellationToken);

        return Result<IReadOnlyList<CustomerAddressResponse>>.Success(
            added.Select(CustomerAddressResponse.From).ToList());
    }
}
