using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.DeactivateLoyaltyTier;

public sealed class DeactivateLoyaltyTierCommandHandler
    : IRequestHandler<DeactivateLoyaltyTierCommand, Result<LoyaltyTierResponse>>
{
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateLoyaltyTierCommandHandler(
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
        DeactivateLoyaltyTierCommand command,
        CancellationToken cancellationToken)
    {
        var tier = await _loyaltyTierRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tier is null)
            return Error.NotFound("loyaltytier.not_found", "Loyalty tier not found.");

        var wasActive = tier.Active;

        var deactivateResult = tier.Deactivate();
        if (deactivateResult.IsFailure)
            return deactivateResult.Error;

        await _loyaltyTierRepository.UpdateAsync(tier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (wasActive)
        {
            var userId = _currentUserService.CurrentUserId ?? "unknown";
            await _auditService.RecordAsync(
                "loyaltytier.deactivated",
                _tenantContext.TenantId,
                $"Loyalty tier '{tier.Id}' deactivated.",
                userId,
                cancellationToken);
        }

        return LoyaltyTierResponse.From(tier);
    }
}
