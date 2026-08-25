using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.SetProductCoverPhoto;

public sealed class SetProductCoverPhotoCommandHandler
    : IRequestHandler<SetProductCoverPhotoCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetProductCoverPhotoCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductResponse>> Handle(
        SetProductCoverPhotoCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        var result = product.SetCoverPhoto(command.PhotoId);
        if (!result.IsSuccess)
            return result.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.From(product);
    }
}
