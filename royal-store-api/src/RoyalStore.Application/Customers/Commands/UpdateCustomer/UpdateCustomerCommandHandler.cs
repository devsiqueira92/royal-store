using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler
    : IRequestHandler<UpdateCustomerCommand, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ILoyaltyTierRepository loyaltyTierRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _customerRepository = customerRepository;
        _loyaltyTierRepository = loyaltyTierRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CustomerResponse>> Handle(
        UpdateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        // Addresses are always loaded: the list is applied to them and the response returns them all
        // (046 FR-021).
        var customer = await _customerRepository.GetByIdWithAddressesAsync(command.Id, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

        var addressChanges = CustomerAddressChanges.From(command.Addresses);
        if (addressChanges.IsFailure)
            return addressChanges.Error;

        var updateResult = customer.Update(command.Name, command.Phone);
        if (updateResult.IsFailure)
            return updateResult.Error;

        if (command.LoyaltyTierId is Guid tierId)
        {
            var tier = await _loyaltyTierRepository.GetByIdAsync(tierId, cancellationToken);
            if (tier is null)
                return Error.Validation("customer.tier_not_found", "Loyalty tier not found.");

            var assignResult = customer.AssignTier(tier.Id);
            if (assignResult.IsFailure)
                return assignResult.Error;
        }

        // New entries are added, edited ones fully replaced; addresses left out are untouched (046 FR-016).
        var applied = customer.ApplyAddressChanges(addressChanges.Value);
        if (applied.IsFailure)
            return applied.Error;

        var changeSet = applied.Value;

        // Explicit Add: a client-assigned key found through the navigation is tracked as Modified,
        // not Added (045 research Decision 5).
        foreach (var address in changeSet.Added)
            await _customerRepository.AddAddressAsync(address, cancellationToken);

        // No UpdateAsync(customer): it's tracked, and Update() would mark the new addresses Modified.
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another address change on this customer committed first (Customer.Version); nothing is
            // saved, the customer's own fields included, so the caller can simply retry.
            return Error.Conflict("customer.concurrent_update",
                "The customer was changed by another operation. Refresh and try again.");
        }

        var currentTier = await _loyaltyTierRepository.GetByIdAsync(customer.LoyaltyTierId, cancellationToken);

        var tenantId = _tenantContext.TenantId;
        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "customer.updated",
            tenantId,
            $"Customer '{customer.Id}' updated.",
            userId,
            cancellationToken);

        // Ids only — an address is personal data and the security log doesn't need its content.
        foreach (var address in changeSet.Added)
            await _auditService.RecordAsync(
                "customer.address_added",
                tenantId,
                $"Address {address.Id} added to customer {customer.Id}.",
                userId,
                cancellationToken);

        foreach (var address in changeSet.Updated)
            await _auditService.RecordAsync(
                "customer.address_updated",
                tenantId,
                $"Address {address.Id} of customer {customer.Id} updated.",
                userId,
                cancellationToken);

        return CustomerResponse.From(customer, currentTier!,
            addresses: CustomerAddressResponse.ListFrom(customer.Addresses));
    }
}
