using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantCommandHandler
    : IRequestHandler<UpdateProductVariantCommand, Result<ProductVariantResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductVariantCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductVariantResponse>> Handle(
        UpdateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        var updateResult = product.UpdateVariant(
            command.VariantId,
            command.Size, command.Color, command.Sku, command.Barcode,
            command.Price, command.CostPrice,
            command.StockQuantity, command.MinStockQuantity, command.IsStockTracked,
            command.PromotionalPrice, command.NetWeight, command.GrossWeight,
            command.Width, command.Height, command.Length);

        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _productRepository.UpdateAsync(product, null, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var variant = product.Variants.First(v => v.Id == command.VariantId);
        return ProductVariantResponse.From(variant);
    }
}
