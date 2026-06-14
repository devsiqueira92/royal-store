using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProductCommandHandler(
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
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.Id, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
            return Error.Validation("product.category_invalid", "Category not found or inactive.");

        if (command.Sku is not null)
        {
            var skuExists = await _productRepository.ExistsBySkuAsync(command.Sku, command.Id, cancellationToken);
            if (skuExists)
                return Error.Conflict("product.sku_conflict", $"A product with SKU '{command.Sku}' already exists.");
        }

        var updateResult = product.Update(
            command.CategoryId, command.Name, command.Description,
            command.Sku, command.Price, command.CostPrice, command.StockQuantity,
            command.MinStockQuantity, command.IsStockTracked, command.ImageUrl,
            command.UnitOfMeasure, command.Barcode, command.Ncm, command.Cest, command.Origin);

        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _productRepository.UpdateAsync(product, cancellationToken);
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
