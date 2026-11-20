using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Labels;

namespace RoyalStore.Application.Labels.Commands.CreateLabel;

public sealed class CreateLabelCommandHandler
    : IRequestHandler<CreateLabelCommand, Result<LabelResponse>>
{
    private readonly ILabelRepository _labelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniqueConstraintDetector _uniqueConstraintDetector;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateLabelCommandHandler> _logger;

    public CreateLabelCommandHandler(
        ILabelRepository labelRepository,
        IUnitOfWork unitOfWork,
        IUniqueConstraintDetector uniqueConstraintDetector,
        ITenantContext tenantContext,
        ILogger<CreateLabelCommandHandler> logger)
    {
        _labelRepository = labelRepository;
        _unitOfWork = unitOfWork;
        _uniqueConstraintDetector = uniqueConstraintDetector;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<LabelResponse>> Handle(
        CreateLabelCommand command, CancellationToken cancellationToken)
    {
        var template = command.Template;

        var created = LabelTemplate.Create(
            _tenantContext.TenantId,
            template.Name,
            command.Format.ToSpec(),
            template.Configuration,
            template.SourceTemplateId,
            template.IsDefault,
            template.IsActive);

        if (created.IsFailure)
            return Refused(created.Error);

        var label = created.Value;

        if (await _labelRepository.ExistsByNormalizedNameAsync(label.NormalizedName, null, cancellationToken))
            return Refused(LabelErrors.NameTaken);

        Guid? previousDefault = null;

        try
        {
            // One transaction: unmarking the previous default and adding this label either both
            // happen or neither does (065 research D7).
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (label.IsDefault)
                    previousDefault = await LabelDefaults.ReleaseAsync(_labelRepository, _unitOfWork, keepId: null, ct);

                await _labelRepository.AddAsync(label, ct);
                await _unitOfWork.SaveChangesAsync(ct);

                return Result<LabelTemplate>.Success(label);
            }, cancellationToken);
        }
        catch (DbUpdateException ex) when (LabelErrors.FromUniqueViolation(_uniqueConstraintDetector, ex) is { } error)
        {
            // A concurrent save took the name or the default between the check and this save.
            return Refused(error);
        }

        _logger.LogInformation(
            "Label {LabelId} created for tenant {TenantId}; default: {IsDefault}, previous default: {PreviousDefaultLabelId}.",
            label.Id, _tenantContext.TenantId, label.IsDefault, previousDefault);

        return LabelResponse.From(label);
    }

    private Error Refused(Error error)
    {
        _logger.LogWarning(
            "Label creation refused for tenant {TenantId}: {ErrorCode}.",
            _tenantContext.TenantId, error.Code);

        return error;
    }
}
