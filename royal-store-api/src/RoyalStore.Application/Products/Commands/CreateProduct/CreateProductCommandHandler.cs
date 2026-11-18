using MediatR;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITaxProfileTemplateRepository _taxProfileTemplateRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly ICostCompositionRepository _costCompositionRepository;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ITaxProfileTemplateRepository taxProfileTemplateRepository,
        ISupplierRepository supplierRepository,
        IPriceListRepository priceListRepository,
        ICostCompositionRepository costCompositionRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ILogger<CreateProductCommandHandler> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _taxProfileTemplateRepository = taxProfileTemplateRepository;
        _supplierRepository = supplierRepository;
        _priceListRepository = priceListRepository;
        _costCompositionRepository = costCompositionRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ProductResponse>> Handle(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

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

        // Every list of the store, active or not: the aggregate decides which ones may be priced.
        var priceLists = await _priceListRepository.GetAllAsync(null, cancellationToken);

        var prices = ProductPriceInputs.ForCreate(command.Prices, command.BasePrice, priceLists);
        if (!prices.IsSuccess)
            return prices.Error;

// Named by identifier. A list that does not exist, or belongs to another store (the tenant filter
        // hides it), is refused before anything is created or changed (064 FR-017).
        CostCompositionList? costComposition = null;
        if (command.CostCompositionId.HasValue)
        {
            costComposition = await _costCompositionRepository.GetByIdAsync(command.CostCompositionId.Value, cancellationToken);
            if (costComposition is null)
                return Error.NotFound("costcomposition.not_found", "Cost composition was not found.");
        }

        var productResult = Product.CreateProduct(
            tenantId, command.CategoryId, command.Name, command.Sku, command.StockTrack,
            prices.Value, priceLists, command.BaseCostPrice, command.Description, command.UnitOfMeasure,
            command.Ncm, command.Cest, command.Origin,
            command.NetWeight, command.GrossWeight, command.Width, command.Height, command.Length,
            taxProfileTemplate, command.OverrideTaxProfile,
            command.DefaultCfop, command.IcmsCst, command.PisCofinsCst, command.IvaCst, command.IsSelectiveTax,
            supplier, costComposition);

        if (!productResult.IsSuccess)
            return productResult.Error;

        var product = productResult.Value;

        foreach (var v in command.Variants ?? [])
        {
            // No sale price: a product variant is priced by the product's price lists (061 FR-024).
            var variantResult = product.AddVariant(tenantId, v.Size, v.Color, v.OptionName, v.Sku, v.Barcode,
                null, v.CostPrice, v.StockQuantity, v.MinStockQuantity);
            if (!variantResult.IsSuccess)
                return variantResult.Error;
        }

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (costComposition is not null)
            _logger.LogInformation(
                "Cost composition {CostCompositionId} linked to product {ProductId} for tenant {TenantId}.",
                costComposition.Id, product.Id, tenantId);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "product.created",
            tenantId,
            $"Product '{product.Name}' created. Id: {product.Id}",
            userId,
            cancellationToken);

        var saved = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
        return ProductResponse.From(saved!);
    }
}
