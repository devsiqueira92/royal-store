using MediatR;
using RoyalStore.Application.Abstractions.Catalog;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Categories.Commands.DisableCategory;

public sealed class DisableCategoryCommandHandler : IRequestHandler<DisableCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICategoryUsageChecker _usageChecker;

    public DisableCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        ICategoryUsageChecker usageChecker)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _usageChecker = usageChecker;
    }

    public async Task<Result> Handle(DisableCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(command.Id, cancellationToken);
        if (category is null)
            return Result.Failure(Error.NotFound("category.not_found", "Category not found."));

        var inUse = await _usageChecker.IsInUseAsync(command.Id, cancellationToken);
        if (inUse)
            return Result.Failure(Error.Conflict("category.in_use",
                "Category cannot be disabled because it is assigned to one or more products."));

        var disableResult = category.Disable();
        if (!disableResult.IsSuccess)
            return disableResult;

        await _categoryRepository.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "category.disabled",
            _tenantContext.TenantId,
            $"Category '{category.Id}' disabled.",
            userId,
            cancellationToken);

        return Result.Success();
    }
}
