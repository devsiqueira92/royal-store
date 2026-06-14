using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductResponse>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        return ProductResponse.From(product);
    }
}
