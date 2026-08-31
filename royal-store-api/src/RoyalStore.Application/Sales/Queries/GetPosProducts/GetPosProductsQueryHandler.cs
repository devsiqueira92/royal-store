using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetPosProducts;

public sealed class GetPosProductsQueryHandler
    : IRequestHandler<GetPosProductsQuery, Result<IReadOnlyList<PosProductDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetPosProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IReadOnlyList<PosProductDto>>> Handle(
        GetPosProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetForPosAsync(request.CategoryId, cancellationToken);

        IReadOnlyList<PosProductDto> response = products
            .SelectMany(product => product.Variants.Select(variant => PosProductDto.From(product, variant)))
            .OrderBy(dto => dto.ProductName)
            .ThenBy(dto => dto.VariantSize)
            .ToList();

        return Result<IReadOnlyList<PosProductDto>>.Success(response);
    }
}
