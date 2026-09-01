using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetPosProducts;

public sealed class GetPosProductsQueryHandler
    : IRequestHandler<GetPosProductsQuery, Result<IReadOnlyList<PosProductDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IIbptTaxRepository _ibptTaxRepository;

    public GetPosProductsQueryHandler(
        IProductRepository productRepository,
        IIbptTaxRepository ibptTaxRepository)
    {
        _productRepository = productRepository;
        _ibptTaxRepository = ibptTaxRepository;
    }

    public async Task<Result<IReadOnlyList<PosProductDto>>> Handle(
        GetPosProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetForPosAsync(request.CategoryId, cancellationToken);

        var codes = products
            .Select(p => p.Ncm)
            .Where(ncm => !string.IsNullOrWhiteSpace(ncm))
            .Select(ncm => ncm!)
            .Distinct()
            .ToList();

        var asOf = DateOnly.FromDateTime(DateTime.UtcNow);
        var ibptByCode = await _ibptTaxRepository.GetCurrentByCodesAsync(codes, asOf, cancellationToken);

        IReadOnlyList<PosProductDto> response = products
            .SelectMany(product => product.Variants.Select(variant => PosProductDto.From(
                product,
                variant,
                product.Ncm is not null && ibptByCode.TryGetValue(product.Ncm, out var ibptTax) ? ibptTax : null)))
            .OrderBy(dto => dto.ProductName)
            .ThenBy(dto => dto.VariantSize)
            .ToList();

        return Result<IReadOnlyList<PosProductDto>>.Success(response);
    }
}
