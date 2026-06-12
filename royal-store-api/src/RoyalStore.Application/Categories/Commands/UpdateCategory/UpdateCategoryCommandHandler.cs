using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Categories.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, Result<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCategoryCommandHandler(
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

    public async Task<Result<CategoryResponse>> Handle(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Error.NotFound("category.not_found", "Category not found.");

        var nameExists = await _categoryRepository.ExistsByNameAsync(command.Name, command.Id, cancellationToken);
        if (nameExists)
            return Error.Conflict("category.name_conflict", $"A category named '{command.Name}' already exists.");

        var updateResult = category.Update(command.Name, command.Description);
        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "category.updated",
            _tenantContext.TenantId,
            $"Category '{category.Id}' updated.",
            userId,
            cancellationToken);

        return CategoryResponse.From(category);
    }
}
