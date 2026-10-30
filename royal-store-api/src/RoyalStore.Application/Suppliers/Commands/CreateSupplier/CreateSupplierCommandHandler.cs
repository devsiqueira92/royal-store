using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Suppliers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Customers;
using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Application.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandHandler
    : IRequestHandler<CreateSupplierCommand, Result<SupplierResponse>>
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateSupplierCommandHandler(
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
        CreateSupplierCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        if (!string.IsNullOrWhiteSpace(command.Cnpj))
        {
            var normalizedCnpj = DocumentValidator.Normalize(command.Cnpj);
            var cnpjExists = await _supplierRepository.ExistsByCnpjAsync(normalizedCnpj, null, cancellationToken);
            if (cnpjExists)
                return Error.Conflict("supplier.cnpj_conflict", $"A supplier with CNPJ '{normalizedCnpj}' already exists.");
        }

        var supplierResult = Supplier.Create(
            tenantId, command.Name, command.Cnpj, command.Phone, command.Address, command.City, command.ZipCode);
        if (!supplierResult.IsSuccess)
            return supplierResult.Error;

        var supplier = supplierResult.Value;

        await _supplierRepository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "supplier.created",
            tenantId,
            $"Supplier '{supplier.Name}' created. Id: {supplier.Id}",
            userId,
            cancellationToken);

        return SupplierResponse.From(supplier);
    }
}
