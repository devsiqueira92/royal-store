using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.RemoveProductVariant;

public sealed class RemoveProductVariantCommandHandler
    : IRequestHandler<RemoveProductVariantCommand, Result>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProductVariantCommandHandler(
        IProductVariantRepository productVariantRepository,
        IUnitOfWork unitOfWork)
    {
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productVariantRepository.GetById(command.VariantId, cancellationToken);
        if (product is null)
            return Result.Failure(Error.NotFound("product.not_found", "Product not found."));


        await _productVariantRepository.RemoveAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
