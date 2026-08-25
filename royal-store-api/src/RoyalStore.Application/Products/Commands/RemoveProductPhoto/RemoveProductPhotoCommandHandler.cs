using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.RemoveProductPhoto;

public sealed class RemoveProductPhotoCommandHandler : IRequestHandler<RemoveProductPhotoCommand, Result>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveProductPhotoCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveProductPhotoCommand command, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Result.Failure(Error.NotFound("product.not_found", "Product not found."));

        var result = product.RemovePhoto(command.PhotoId);
        if (!result.IsSuccess)
            return result;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
