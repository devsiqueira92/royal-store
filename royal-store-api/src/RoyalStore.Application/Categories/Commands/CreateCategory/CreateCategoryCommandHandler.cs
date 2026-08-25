using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Categories.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Result<CategoryResponse>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateCategoryCommandHandler(
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
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var nameExists = await _categoryRepository.ExistsByNameAsync(command.Name, null, cancellationToken);
        if (nameExists)
            return Error.Conflict("category.name_conflict", $"A category named '{command.Name}' already exists.");

        var categoryResult = Category.Create(tenantId, command.Name, command.Description, command.Color);
        if (!categoryResult.IsSuccess)
            return categoryResult.Error;

        var category = categoryResult.Value;

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "category.created",
            tenantId,
            $"Category '{category.Name}' created. Id: {category.Id}",
            userId,
            cancellationToken);

        return CategoryResponse.From(category);
    }
}
