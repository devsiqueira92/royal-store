using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Customers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Customers;

namespace RoyalStore.Application.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler
    : IRequestHandler<CreateCustomerCommand, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateCustomerCommandHandler(
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
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Formatting-insensitive duplicate check (FR-003/FR-004): compare normalized digits, the same
        // form the document is ultimately stored in.
        var normalizedDocument = DocumentValidator.Normalize(command.Document);
        var documentExists = await _customerRepository.ExistsByDocumentAsync(normalizedDocument, null, cancellationToken);
        if (documentExists)
            return Error.Conflict("customer.document_conflict",
                $"A customer with document '{command.Document}' is already registered.");

        // Optional address, saved with the customer or not at all (045 FR-003). Validated before the
        // tier lookup so a bad field fails fast; an all-blank address counts as none (FR-011a).
        PostalAddress? postal = null;
        if (command.Address is { } addressRequest)
        {
            var postalResult = PostalAddress.Create(addressRequest.ZipCode, addressRequest.Street,
                addressRequest.Number, addressRequest.Neighborhood, addressRequest.City, addressRequest.State,
                addressRequest.Complement, addressRequest.AdditionalInfo);
            if (postalResult.IsFailure)
                return postalResult.Error;

            postal = postalResult.Value;
        }

        var tier = command.LoyaltyTierId is Guid explicitTierId
            ? await _loyaltyTierRepository.GetByIdAsync(explicitTierId, cancellationToken)
            : await _loyaltyTierRepository.GetStandardTierAsync(tenantId, cancellationToken);

        if (tier is null)
            return Error.Validation("customer.tier_not_found", "Loyalty tier not found.");

        var customerResult = Customer.Create(
            tenantId, command.Name, command.Document, command.Phone, termsAccepted: false, tier.Id);
        if (customerResult.IsFailure)
            return customerResult.Error;

        var customer = customerResult.Value;

        // First address, so it always becomes the default (FR-010). AddAsync below inserts the whole graph.
        if (postal is { IsEmpty: false })
        {
            var addressResult = customer.AddAddress(Guid.NewGuid(), postal, command.Address!.IsDefaultDelivery);
            if (addressResult.IsFailure)
                return addressResult.Error;
        }

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "customer.created",
            tenantId,
            $"Customer '{customer.Name}' created. Id: {customer.Id}",
            userId,
            cancellationToken);

        // Ids only — an address is personal data and the security log doesn't need its content.
        foreach (var address in customer.Addresses)
            await _auditService.RecordAsync(
                "customer.address_added",
                tenantId,
                $"Address {address.Id} added to customer {customer.Id}.",
                userId,
                cancellationToken);

        return CustomerResponse.From(customer, tier, cashbackBalance: 0m,
            addresses: customer.Addresses.Select(CustomerAddressResponse.From).ToList());
    }
}
