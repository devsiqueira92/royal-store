using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Application.Services.Commands.EditService;

public sealed class EditServiceCommandHandler
    : IRequestHandler<EditServiceCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITaxProfileTemplateRepository _taxProfileTemplateRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public EditServiceCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ITaxProfileTemplateRepository taxProfileTemplateRepository,
        ISupplierRepository supplierRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _taxProfileTemplateRepository = taxProfileTemplateRepository;
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProductResponse>> Handle(
        EditServiceCommand command,
        CancellationToken cancellationToken)
    {
        var service = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (service is null || service.Type != ItemType.Service)
            return Error.NotFound("service.not_found", "Service not found.");

        if (command.Version != service.Version)
            return Error.Conflict("service.stale", "The service was modified by someone else. Reload and try again.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
            return Error.Validation("product.category_invalid", "Category not found or inactive.");

        var taxProfileTemplate = await _taxProfileTemplateRepository.GetByIdAsync(command.TaxProfileTemplateId, cancellationToken);
        if (taxProfileTemplate is null || !taxProfileTemplate.IsActive)
            return Error.NotFound("product.tax_profile_template_not_found", "Tax profile template not found or inactive.");

        Supplier? supplier = null;
        if (command.SupplierId.HasValue)
        {
            supplier = await _supplierRepository.GetByIdAsync(command.SupplierId.Value, cancellationToken);
            if (supplier is null || !supplier.IsActive)
                return Error.Validation("product.supplier_invalid", "Supplier not found or inactive.");
        }

        var variantsToAdd = command.VariantsToAdd
            .Select(v => new NewVariantRequest(null, null, v.OptionName, null, null, v.Price, v.CostPrice, null, null))
            .ToList();

        var variantsToUpdate = command.VariantsToUpdate
            .Select(v => new VariantUpdateRequest(v.VariantId, null, null, v.OptionName, null, null, v.Price, v.CostPrice, null, null))
            .ToList();

        var existingVariantIds = service.Variants.Select(v => v.Id).ToHashSet();

        var updateResult = service.UpdateServiceWithVariants(
            command.CategoryId, command.Name, command.Sku,
            command.BasePrice, command.BaseCostPrice, command.Description, command.UnitOfMeasure,
            command.Nbs, command.IssCode,
            taxProfileTemplate, command.OverrideTaxProfile,
            command.PisCofinsCst, command.IvaCst, command.IsSelectiveTax,
            variantsToAdd, variantsToUpdate, supplier);

        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _productRepository.UpdateAsync(service, command.Version, cancellationToken);

        var newVariants = service.Variants.Where(v => !existingVariantIds.Contains(v.Id));
        foreach (var variant in newVariants)
            await _productRepository.AddVariantAsync(variant, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "service.updated",
            _tenantContext.TenantId,
            $"Service '{service.Id}' updated.",
            userId,
            cancellationToken);

        return ProductResponse.From(service);
    }
}
