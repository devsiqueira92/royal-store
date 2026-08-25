using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.AssignProductSizes;

public sealed class AssignProductSizesCommandHandler
    : IRequestHandler<AssignProductSizesCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignProductSizesCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductResponse>> Handle(
        AssignProductSizesCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        var sizes = command.Sizes
            .Select(s => Enum.Parse<SizeValue>(s, ignoreCase: true))
            .ToList();

        var result = product.AssignSizes(sizes);
        if (!result.IsSuccess)
            return result.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.From(product);
    }
}
