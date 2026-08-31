using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Commands.DeactivateSeller;

public sealed class DeactivateSellerCommandHandler : IRequestHandler<DeactivateSellerCommand, Result>
{
    private readonly ISellerRepository _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateSellerCommandHandler(
        ISellerRepository sellerRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeactivateSellerCommand command, CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (seller is null)
            return Result.Failure(Error.NotFound("seller.not_found", "Seller not found."));

        var deactivateResult = seller.Deactivate();
        if (!deactivateResult.IsSuccess)
            return deactivateResult;

        await _sellerRepository.UpdateAsync(seller, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "seller.deactivated",
            _tenantContext.TenantId,
            deactivateResult.Error.Message,
            userId,
            cancellationToken);

        return Result.Success();
    }
}
