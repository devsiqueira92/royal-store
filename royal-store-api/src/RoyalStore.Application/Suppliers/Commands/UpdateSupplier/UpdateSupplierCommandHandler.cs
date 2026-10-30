using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Suppliers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Customers;

namespace RoyalStore.Application.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler
    : IRequestHandler<UpdateSupplierCommand, Result<SupplierResponse>>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateSupplierCommandHandler(
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

    public async Task<Result<SupplierResponse>> Handle(
        UpdateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        var supplier = await _supplierRepository.GetByIdAsync(command.Id, cancellationToken);
        if (supplier is null)
            return Error.NotFound("supplier.not_found", "Supplier not found.");

        if (!string.IsNullOrWhiteSpace(command.Cnpj))
        {
            var normalizedCnpj = DocumentValidator.Normalize(command.Cnpj);
            var cnpjExists = await _supplierRepository.ExistsByCnpjAsync(normalizedCnpj, command.Id, cancellationToken);
            if (cnpjExists)
                return Error.Conflict("supplier.cnpj_conflict", $"A supplier with CNPJ '{normalizedCnpj}' already exists.");
        }

        var updateResult = supplier.Update(
            command.Name, command.Cnpj, command.Phone, command.Address, command.City, command.ZipCode);
        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _supplierRepository.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "supplier.updated",
            _tenantContext.TenantId,
            $"Supplier '{supplier.Id}' updated.",
            userId,
            cancellationToken);

        return SupplierResponse.From(supplier);
    }
}
