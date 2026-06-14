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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
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

        if (command.Sku is not null)
        {
            var skuExists = await _productRepository.ExistsBySkuAsync(command.Sku, null, cancellationToken);
            if (skuExists)
                return Error.Conflict("product.sku_conflict", $"A product with SKU '{command.Sku}' already exists.");
        }

        var productResult = Product.Create(
            tenantId, command.CategoryId, command.Name, command.Description,
            command.Sku, command.Price, command.CostPrice, command.StockQuantity,
            command.MinStockQuantity, command.IsStockTracked, command.ImageUrl,
            command.UnitOfMeasure, command.Barcode, command.Ncm, command.Cest, command.Origin);

        if (!productResult.IsSuccess)
            return productResult.Error;

        var product = productResult.Value;

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "product.created",
            tenantId,
            $"Product '{product.Name}' created. Id: {product.Id}",
            userId,
            cancellationToken);

        return ProductResponse.From(product);
    }
}
