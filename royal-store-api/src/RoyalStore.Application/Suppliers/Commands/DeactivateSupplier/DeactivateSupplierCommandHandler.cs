using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Suppliers.Commands.DeactivateSupplier;

public sealed class DeactivateSupplierCommandHandler : IRequestHandler<DeactivateSupplierCommand, Result>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateSupplierCommandHandler(
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeactivateSupplierCommand command, CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(command.Id, cancellationToken);
        if (supplier is null)
            return Result.Failure(Error.NotFound("supplier.not_found", "Supplier not found."));

        var deactivateResult = supplier.Deactivate();
        if (!deactivateResult.IsSuccess)
            return deactivateResult;

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "supplier.deactivated",
            _tenantContext.TenantId,
            $"Supplier '{supplier.Id}' deactivated.",
            userId,
            cancellationToken);

        return Result.Success();
    }
}
