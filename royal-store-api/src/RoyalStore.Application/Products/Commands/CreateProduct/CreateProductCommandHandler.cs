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

        var productResult = Product.Create(
            tenantId, command.CategoryId, command.Name, command.ReferenceCode, command.Description,
            command.UnitOfMeasure, command.Ncm, command.Cest, command.Origin,
            command.Type, command.Nbs, command.IssCode, command.DefaultCfop,
            command.IcmsCst, command.PisCofinsCst, command.IvaCst, command.IsSelectiveTax);

        if (!productResult.IsSuccess)
            return productResult.Error;

        var product = productResult.Value;

        foreach (var v in command.Variants ?? [])
        {
            var variantResult = product.AddVariant(tenantId, v.Size, v.Color, v.Sku, v.Barcode,
                v.Price, v.CostPrice, v.StockQuantity, v.MinStockQuantity, v.IsStockTracked,
                v.PromotionalPrice, v.NetWeight, v.GrossWeight, v.Width, v.Height, v.Length);
            if (!variantResult.IsSuccess)
                return variantResult.Error;
        }

        if (command.TagIds is { Count: > 0 })
        {
            foreach (var tagId in command.TagIds)
            {
                var tagCategory = await _categoryRepository.GetByIdAsync(tagId, cancellationToken);
                if (tagCategory is null)
                    return Error.Validation("product.tag_invalid", $"Tag category '{tagId}' not found.");
            }

            product.AssignTags(command.TagIds);
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
