using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.TaxProfileTemplates.Commands.DeactivateTaxProfileTemplate;

public sealed class DeactivateTaxProfileTemplateCommandHandler : IRequestHandler<DeactivateTaxProfileTemplateCommand, Result>
{
    private readonly ITaxProfileTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateTaxProfileTemplateCommandHandler(
        ITaxProfileTemplateRepository repository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(DeactivateTaxProfileTemplateCommand command, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (template is null)
            return Result.Failure(Error.NotFound("tax_profile_template.not_found", "Tax profile template not found."));

        // Deactivation is always allowed, even when the template is still referenced by
        // products/services — it only stops the template from being offered to new ones.
        var deactivateResult = template.Deactivate();
        if (!deactivateResult.IsSuccess)
            return deactivateResult;

        await _repository.UpdateAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "tax_profile_template.deactivated",
            _tenantContext.TenantId,
            $"Tax profile template '{template.Id}' deactivated.",
            userId,
            cancellationToken);

        return Result.Success();
    }
}
