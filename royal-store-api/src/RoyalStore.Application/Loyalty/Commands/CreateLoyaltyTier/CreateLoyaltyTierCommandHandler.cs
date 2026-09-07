using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Loyalty;

namespace RoyalStore.Application.Loyalty.Commands.CreateLoyaltyTier;

public sealed class CreateLoyaltyTierCommandHandler
    : IRequestHandler<CreateLoyaltyTierCommand, Result<LoyaltyTierResponse>>
{
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateLoyaltyTierCommandHandler(
        ILoyaltyTierRepository loyaltyTierRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _loyaltyTierRepository = loyaltyTierRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<LoyaltyTierResponse>> Handle(
        CreateLoyaltyTierCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var nameExists = await _loyaltyTierRepository.ExistsByNameAsync(command.Name, null, cancellationToken);
        if (nameExists)
            return Error.Conflict("loyaltytier.name_conflict", $"A tier named '{command.Name}' already exists.");

        var tierResult = LoyaltyTier.Create(tenantId, command.Name, command.CashbackPercentage, command.CashbackValidityDays);
        if (tierResult.IsFailure)
            return tierResult.Error;

        var tier = tierResult.Value;

        await _loyaltyTierRepository.AddAsync(tier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "loyaltytier.created",
            tenantId,
            $"Loyalty tier '{tier.Name}' created. Id: {tier.Id}",
            userId,
            cancellationToken);

        return LoyaltyTierResponse.From(tier);
    }
}
