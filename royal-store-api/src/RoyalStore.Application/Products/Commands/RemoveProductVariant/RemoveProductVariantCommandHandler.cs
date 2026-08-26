using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.RemoveProductVariant;

public sealed class RemoveProductVariantCommandHandler
    : IRequestHandler<RemoveProductVariantCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProductVariantCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure(Error.NotFound("product.not_found", "Product not found."));

        var removeResult = product.RemoveVariant(command.VariantId);
        if (!removeResult.IsSuccess)
            return removeResult;

        await _productRepository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
