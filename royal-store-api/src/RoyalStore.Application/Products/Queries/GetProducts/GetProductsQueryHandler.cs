using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, Result<IReadOnlyList<ProductResponse>>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyList<ProductResponse>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(request.IsActive, request.CategoryId, cancellationToken);
        IReadOnlyList<ProductResponse> response = products.Select(ProductResponse.From).ToList();
        return Result<IReadOnlyList<ProductResponse>>.Success(response);
    }
}
