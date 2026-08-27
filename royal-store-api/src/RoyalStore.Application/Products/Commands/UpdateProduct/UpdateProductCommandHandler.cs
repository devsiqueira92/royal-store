using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
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

        if (command.Version != product.Version)
            return Error.Conflict("product.stale", "The product was modified by someone else. Reload and try again.");

        var category = await _categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
        if (category is null || !category.IsActive)
            return Error.Validation("product.category_invalid", "Category not found or inactive.");

        var variantsToAdd = command.VariantsToAdd
            .Select(v => new NewVariantRequest(
                v.Size, v.Color, v.Sku, v.Barcode,
                v.Price, v.CostPrice, v.StockQuantity, v.MinStockQuantity, v.IsStockTracked))
            .ToList();

        var variantsToUpdate = command.VariantsToUpdate
            .Select(v => new VariantUpdateRequest(
                v.VariantId, v.Size, v.Color, v.Sku, v.Barcode,
                v.Price, v.CostPrice, v.StockQuantity, v.MinStockQuantity, v.IsStockTracked))
            .ToList();

        var existingVariantIds = product.Variants.Select(v => v.Id).ToHashSet();

        var updateResult = product.UpdateWithVariants(
            command.CategoryId, command.Name, command.ReferenceCode, command.Description,
            command.UnitOfMeasure, command.Ncm, command.Cest, command.Origin,
            variantsToAdd, variantsToUpdate);

        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _productRepository.UpdateAsync(product, command.Version, cancellationToken);

        // UpdateWithVariants appends brand-new ProductVariant entities to the already-tracked
        // Product.Variants collection. EF's change tracker can't tell those apart from existing
        // rows on its own — their Id is a client-generated Guid set before insertion — so left to
        // automatic detection it marks them Modified instead of Added, producing a spurious
        // 0-rows-affected concurrency failure. Explicitly track each newly-created variant as
        // Added, same as the standalone add-variant endpoint does.
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
