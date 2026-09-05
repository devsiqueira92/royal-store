using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.TaxProfileTemplates.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.TaxProfileTemplates.Commands.CreateTaxProfileTemplate;

public sealed class CreateTaxProfileTemplateCommandHandler
    : IRequestHandler<CreateTaxProfileTemplateCommand, Result<TaxProfileTemplateResponse>>
{
    private readonly ITaxProfileTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateTaxProfileTemplateCommandHandler(
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
        CreateTaxProfileTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var templateResult = TaxProfileTemplate.Create(
            tenantId, command.Name, command.Description, command.TargetItemType,
            command.DefaultCfop, command.IcmsCst, command.PisCofinsCst, command.IvaCst,
            command.IsSelectiveTax, command.IsDefault);

        if (!templateResult.IsSuccess)
            return templateResult.Error;

        var template = templateResult.Value;

        await _repository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "tax_profile_template.created",
            tenantId,
            $"Tax profile template '{template.Name}' created. Id: {template.Id}",
            userId,
            cancellationToken);

        return TaxProfileTemplateResponse.From(template);
    }
}
