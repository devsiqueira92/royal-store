using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("category.not_found", "Category not found."));

        var hasProducts = await _categoryRepository.HasProductsAsync(command.Id, cancellationToken);
        if (hasProducts)
            return Result.Failure(Error.Conflict("category.has_products",
                "Category cannot be deleted because it has associated products."));

        await _categoryRepository.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "category.deleted",
            _tenantContext.TenantId,
            $"Category '{category.Id}' deleted.",
            userId,
            cancellationToken);

        return Result.Success();
    }
}
