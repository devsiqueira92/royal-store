using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.AddProductVariant;

public sealed class AddProductVariantCommandHandler
    : IRequestHandler<AddProductVariantCommand, Result<ProductVariantResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AddProductVariantCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<ProductVariantResponse>> Handle(
        AddProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        var variantResult = product.AddVariant(
            _tenantContext.TenantId,
            command.Size, command.Color, command.Sku, command.Barcode,
            command.Price, command.CostPrice,
            command.StockQuantity, command.MinStockQuantity, command.IsStockTracked);

        if (!variantResult.IsSuccess)
            return variantResult.Error;

        await _productRepository.AddVariantAsync(variantResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductVariantResponse.From(variantResult.Value);
    }
}
