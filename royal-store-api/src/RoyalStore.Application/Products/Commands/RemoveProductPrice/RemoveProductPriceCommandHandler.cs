using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.RemoveProductPrice;

public sealed class RemoveProductPriceCommandHandler
    : IRequestHandler<RemoveProductPriceCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProductPriceCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null || product.Type != ItemType.Product)
            return Result.Failure(Error.NotFound("product.not_found", "Product not found."));

        var removed = product.RemovePrice(command.PriceListId);
        if (!removed.IsSuccess)
            return Result.Failure(removed.Error);

        _productRepository.RemovePrice(removed.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
