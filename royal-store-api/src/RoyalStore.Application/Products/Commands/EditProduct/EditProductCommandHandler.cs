using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Application.Products.Commands.EditProduct;

public sealed class EditProductCommandHandler
    : IRequestHandler<EditProductCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITaxProfileTemplateRepository _taxProfileTemplateRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public EditProductCommandHandler(
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
        EditProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        if (product.Type != ItemType.Product)
            return Error.NotFound("product.not_found", "Product not found.");

        if (command.Version != product.Version)
            return Error.Conflict("product.stale", "The product was modified by someone else. Reload and try again.");

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
            .Select(v => new NewVariantRequest(
                v.Size, v.Color, v.OptionName, v.Sku, v.Barcode, v.Price, v.CostPrice, v.StockQuantity, v.MinStockQuantity))
            .ToList();

        var variantsToUpdate = command.VariantsToUpdate
            .Select(v => new VariantUpdateRequest(
                v.VariantId, v.Size, v.Color, v.OptionName, v.Sku, v.Barcode, v.Price, v.CostPrice, v.StockQuantity, v.MinStockQuantity))
            .ToList();

        var existingVariantIds = product.Variants.Select(v => v.Id).ToHashSet();

        var updateResult = product.UpdateProductWithVariants(
            command.CategoryId, command.Name, command.Sku, command.StockTrack,
            command.BasePrice, command.BaseCostPrice, command.Description, command.UnitOfMeasure,
            command.Ncm, command.Cest, command.Origin,
            command.Measure.NetWeight, command.Measure.GrossWeight, command.Measure.Width, command.Measure.Height, command.Measure.Length,
            taxProfileTemplate, command.OverrideTaxProfile,
            command.DefaultCfop, command.IcmsCst, command.PisCofinsCst, command.IvaCst, command.IsSelectiveTax,
            variantsToAdd, variantsToUpdate, supplier);

        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _productRepository.UpdateAsync(product, command.Version, cancellationToken);

        var newVariants = product.Variants.Where(v => !existingVariantIds.Contains(v.Id));
        foreach (var variant in newVariants)
            await _productRepository.AddVariantAsync(variant, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "product.updated",
            _tenantContext.TenantId,
            $"Product '{product.Id}' updated.",
            userId,
            cancellationToken);

        return ProductResponse.From(product);
    }
}
