using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Labels;

namespace RoyalStore.Application.Labels.Commands.UpdateLabel;

public sealed class UpdateLabelCommandHandler
    : IRequestHandler<UpdateLabelCommand, Result<LabelResponse>>
{
    private readonly ILabelRepository _labelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniqueConstraintDetector _uniqueConstraintDetector;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<UpdateLabelCommandHandler> _logger;

    public UpdateLabelCommandHandler(
        ILabelRepository labelRepository,
        IUnitOfWork unitOfWork,
        IUniqueConstraintDetector uniqueConstraintDetector,
        ITenantContext tenantContext,
        ILogger<UpdateLabelCommandHandler> logger)
    {
        _labelRepository = labelRepository;
        _unitOfWork = unitOfWork;
        _uniqueConstraintDetector = uniqueConstraintDetector;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<LabelResponse>> Handle(
        UpdateLabelCommand command, CancellationToken cancellationToken)
    {
        var label = await _labelRepository.GetByIdAsync(command.Id, cancellationToken);
        if (label is null)
            return Refused(command.Id, LabelErrors.NotFound);

        // 066 FR-006: refused before any other check, and before a default swap could start.
        if (label.IsSystem)
            return Refused(label.Id, LabelTemplate.SystemReadOnly);

        var template = command.Template;

        // 065 FR-013: the format sent must be this label's own.
        if (command.Format.Id != label.LabelFormatId ||
            (template.LabelFormatId is { } linked && linked != label.LabelFormatId))
            return Refused(label.Id, LabelErrors.FormatMismatch);

        if (await _labelRepository.ExistsByNormalizedNameAsync(
                LabelTemplate.Normalize(template.Name), label.Id, cancellationToken))
            return Refused(label.Id, LabelErrors.NameTaken);

        Guid? previousDefault = null;
        Result<LabelTemplate> result;

        try
        {
            // One transaction: unmarking the previous default and changing this label either both
            // happen or neither does (065 research D7). A refused change rolls the unmark back.
            result = await _unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                if (template.IsDefault && !label.IsDefault)
                    previousDefault = await LabelDefaults.ReleaseAsync(_labelRepository, _unitOfWork, keepId: label.Id, ct);

                var updated = label.Update(
                    template.Name,
                    command.Format.ToSpec(),
                    template.Configuration,
                    template.SourceTemplateId,
                    template.IsDefault,
                    template.IsActive);

                if (updated.IsFailure)
                    return Result<LabelTemplate>.Failure(updated.Error);

                await _unitOfWork.SaveChangesAsync(ct);
                return Result<LabelTemplate>.Success(label);
            }, cancellationToken);
        }
        catch (DbUpdateException ex) when (LabelErrors.FromUniqueViolation(_uniqueConstraintDetector, ex) is { } error)
        {
            // A concurrent save took the name or the default between the check and this save.
            return Refused(label.Id, error);
        }

        if (result.IsFailure)
            return Refused(label.Id, result.Error);

        _logger.LogInformation(
            "Label {LabelId} changed for tenant {TenantId}; default: {IsDefault}, active: {IsActive}, previous default: {PreviousDefaultLabelId}.",
            label.Id, _tenantContext.TenantId, label.IsDefault, label.IsActive, previousDefault);

        return LabelResponse.From(label);
    }

    private Error Refused(Guid labelId, Error error)
    {
        _logger.LogWarning(
            "Label {LabelId} change refused for tenant {TenantId}: {ErrorCode}.",
            labelId, _tenantContext.TenantId, error.Code);

        return error;
    }
}
