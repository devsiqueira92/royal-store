using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.DisableProduct;

public sealed class DisableProductCommandHandler : IRequestHandler<DisableProductCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DisableProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DisableProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Result.Failure(Error.NotFound("product.not_found", "Product not found."));

        var disableResult = product.Disable();
        if (!disableResult.IsSuccess)
            return disableResult;

        await _productRepository.UpdateAsync(product, null, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "product.disabled",
            _tenantContext.TenantId,
            $"Product '{product.Id}' disabled.",
            userId,
            cancellationToken);

        return Result.Success();
    }
}
