using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.TaxProfileTemplates.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.TaxProfileTemplates.Commands.UpdateTaxProfileTemplate;

public sealed class UpdateTaxProfileTemplateCommandHandler
    : IRequestHandler<UpdateTaxProfileTemplateCommand, Result<TaxProfileTemplateResponse>>
{
    private readonly ITaxProfileTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateTaxProfileTemplateCommandHandler(
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

    public async Task<Result<TaxProfileTemplateResponse>> Handle(
        UpdateTaxProfileTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (template is null)
            return Error.NotFound("tax_profile_template.not_found", "Tax profile template not found.");

        var updateResult = template.Update(
            command.Name, command.Description,
            command.DefaultCfop, command.IcmsCst, command.PisCofinsCst, command.IvaCst,
            command.IsSelectiveTax, command.IsDefault);

        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _repository.UpdateAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "tax_profile_template.updated",
            _tenantContext.TenantId,
            $"Tax profile template '{template.Id}' updated.",
            userId,
            cancellationToken);

        return TaxProfileTemplateResponse.From(template);
    }
}
