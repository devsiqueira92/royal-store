using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Catalog.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Catalog.Queries.GetCatalog;

public sealed class GetCatalogQueryHandler
    : IRequestHandler<GetCatalogQuery, Result<CatalogSyncResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;

    public GetCatalogQueryHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<CatalogSyncResponse>> Handle(
        GetCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var productsTask = _productRepository.GetForCatalogAsync(request.Since, cancellationToken);
        var categoriesTask = _categoryRepository.GetForCatalogAsync(request.Since, cancellationToken);

        await Task.WhenAll(productsTask, categoriesTask);

        IReadOnlyList<CatalogProductDto> products =
            productsTask.Result.Select(CatalogProductDto.From).ToList();

        IReadOnlyList<CatalogCategoryDto> categories =
            categoriesTask.Result.Select(CatalogCategoryDto.From).ToList();

        return Result<CatalogSyncResponse>.Success(new CatalogSyncResponse(products, categories));
    }
}
