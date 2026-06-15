using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Commands.UpdateSeller;

public sealed class UpdateSellerCommandHandler
    : IRequestHandler<UpdateSellerCommand, Result<SellerResponse>>
{
    private readonly ISellerRepository _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSellerCommandHandler(
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

    public async Task<Result<SellerResponse>> Handle(
        UpdateSellerCommand command,
        CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetByIdAsync(command.Id, cancellationToken);
        if (seller is null)
            return Error.NotFound("seller.not_found", "Seller not found.");

        var updateResult = seller.Update(command.FullName, command.Phone, command.UserId);
        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _sellerRepository.UpdateAsync(seller, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "seller.updated",
            _tenantContext.TenantId,
            string.Empty,
            seller.Cpf,
            cancellationToken);

        return SellerResponse.From(seller);
    }
}
