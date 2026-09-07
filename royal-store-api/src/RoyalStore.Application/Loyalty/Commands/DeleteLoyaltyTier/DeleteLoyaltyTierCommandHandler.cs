using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.DeleteLoyaltyTier;

public sealed class DeleteLoyaltyTierCommandHandler : IRequestHandler<DeleteLoyaltyTierCommand, Result>
{
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly ILoyaltyTierUsageChecker _usageChecker;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteLoyaltyTierCommandHandler(
        ILoyaltyTierRepository loyaltyTierRepository,
        ILoyaltyTierUsageChecker usageChecker,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _loyaltyTierRepository = loyaltyTierRepository;
        _usageChecker = usageChecker;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteLoyaltyTierCommand command, CancellationToken cancellationToken)
    {
        var tier = await _loyaltyTierRepository.GetByIdAsync(command.Id, cancellationToken);
        if (tier is null)
            return Result.Failure(Error.NotFound("loyaltytier.not_found", "Loyalty tier not found."));

        var inUse = await _usageChecker.IsInUseAsync(command.Id, cancellationToken);
        if (inUse)
            return Result.Failure(Error.Conflict("loyaltytier.in_use",
                "Tier cannot be removed because it is assigned to one or more customers."));

        await _loyaltyTierRepository.DeleteAsync(tier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "loyaltytier.deleted",
            _tenantContext.TenantId,
            $"Loyalty tier '{tier.Id}' deleted.",
            userId,
            cancellationToken);

        return Result.Success();
    }
}
