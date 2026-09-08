using MediatR;
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
        var customer = await _customerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (customer is null)
            return Error.NotFound("customer.not_found", "Customer not found.");

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

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var currentTier = await _loyaltyTierRepository.GetByIdAsync(customer.LoyaltyTierId, cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "customer.updated",
            _tenantContext.TenantId,
            $"Customer '{customer.Id}' updated.",
            userId,
            cancellationToken);

        return CustomerResponse.From(customer, currentTier!);
    }
}
