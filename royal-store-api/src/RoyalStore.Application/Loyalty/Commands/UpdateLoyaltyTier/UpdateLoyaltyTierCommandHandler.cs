using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.UpdateLoyaltyTier;

public sealed class UpdateLoyaltyTierCommandHandler
    : IRequestHandler<UpdateLoyaltyTierCommand, Result<LoyaltyTierResponse>>
{
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLoyaltyTierCommandHandler(
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
        UpdateLoyaltyTierCommand command,
        CancellationToken cancellationToken)
    {
        var tier = await _loyaltyTierRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tier is null)
            return Error.NotFound("loyaltytier.not_found", "Loyalty tier not found.");

        // Only future accrual is affected — cashback already earned keeps the value and expiry it was
        // granted with (FR-015, FR-019a). Nothing here touches CashbackEntries.
        var updateResult = tier.Update(command.CashbackPercentage, command.CashbackValidityDays);
        if (updateResult.IsFailure)
            return updateResult.Error;

        await _loyaltyTierRepository.UpdateAsync(tier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "loyaltytier.updated",
            _tenantContext.TenantId,
            $"Loyalty tier '{tier.Id}' updated.",
            userId,
            cancellationToken);

        return LoyaltyTierResponse.From(tier);
    }
}
