using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.AddProductPhoto;

public sealed class AddProductPhotoCommandHandler
    : IRequestHandler<AddProductPhotoCommand, Result<ProductPhotoResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AddProductPhotoCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<Result<ProductPhotoResponse>> Handle(
        AddProductPhotoCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        var photoResult = product.AddPhoto(_tenantContext.TenantId, command.Url);
        if (!photoResult.IsSuccess)
            return photoResult.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var photo = photoResult.Value;
        return new ProductPhotoResponse(photo.Id, photo.Url, photo.IsCover);
    }
}
