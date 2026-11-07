using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Queries.GetProductsSummary;

public sealed class GetProductsSummaryQueryHandler
    : IRequestHandler<GetProductsSummaryQuery, Result<ProductsSummaryResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsSummaryQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductsSummaryResponse>> Handle(
        GetProductsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var summary = await _productRepository.GetSummaryAsync(
            request.CategoryId, request.SupplierId, cancellationToken);
        return Result<ProductsSummaryResponse>.Success(ProductsSummaryResponse.From(summary));
    }
}
