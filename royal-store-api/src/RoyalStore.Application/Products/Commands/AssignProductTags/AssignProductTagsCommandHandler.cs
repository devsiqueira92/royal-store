using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products.Commands.AssignProductTags;

public sealed class AssignProductTagsCommandHandler
    : IRequestHandler<AssignProductTagsCommand, Result<ProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignProductTagsCommandHandler(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ProductResponse>> Handle(
        AssignProductTagsCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product is null)
            return Error.NotFound("product.not_found", "Product not found.");

        foreach (var tagId in command.TagIds.Distinct())
        {
            var category = await _categoryRepository.GetByIdAsync(tagId, cancellationToken);
            if (category is null)
                return Error.Validation("product.tag_invalid", $"Tag category '{tagId}' not found for this tenant.");
        }

        product.AssignTags(command.TagIds);

        await _productRepository.UpdateAsync(product, null, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _productRepository.GetByIdAsync(product.Id, cancellationToken);
        return ProductResponse.From(saved!);
    }
}
