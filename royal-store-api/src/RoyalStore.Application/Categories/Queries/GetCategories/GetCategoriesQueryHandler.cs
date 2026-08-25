using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Categories.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler
    : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<CategoryResponse> response = categories
            .Select(c => CategoryResponse.From(c.Category, c.ProductCount))
            .ToList();
        return Result<IReadOnlyList<CategoryResponse>>.Success(response);
    }
}
