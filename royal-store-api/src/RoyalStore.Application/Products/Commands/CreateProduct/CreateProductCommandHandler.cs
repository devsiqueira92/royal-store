using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITaxProfileTemplateRepository _taxProfileTemplateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ITaxProfileTemplateRepository taxProfileTemplateRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _taxProfileTemplateRepository = taxProfileTemplateRepository;
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

        var productResult = Product.CreateProduct(
            tenantId, command.CategoryId, command.Name, command.Sku, command.StockTrack,
            command.BasePrice, command.BaseCostPrice, command.Description, command.UnitOfMeasure,
            command.Ncm, command.Cest, command.Origin,
            command.NetWeight, command.GrossWeight, command.Width, command.Height, command.Length,
            taxProfileTemplate, command.OverrideTaxProfile,
            command.DefaultCfop, command.IcmsCst, command.PisCofinsCst, command.IvaCst, command.IsSelectiveTax);

        if (!productResult.IsSuccess)
            return productResult.Error;

        var product = productResult.Value;

        foreach (var v in command.Variants ?? [])
        {
            var variantResult = product.AddVariant(tenantId, v.Size, v.Color, v.OptionName, v.Sku, v.Barcode,
                v.Price, v.CostPrice, v.StockQuantity, v.MinStockQuantity);
            if (!variantResult.IsSuccess)
                return variantResult.Error;
        }

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
