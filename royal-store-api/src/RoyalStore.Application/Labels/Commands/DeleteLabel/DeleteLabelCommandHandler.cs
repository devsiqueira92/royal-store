using MediatR;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Commands.DeleteLabel;

public sealed class DeleteLabelCommandHandler : IRequestHandler<DeleteLabelCommand, Result>
{
    private readonly ILabelRepository _labelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteLabelCommandHandler> _logger;

    public DeleteLabelCommandHandler(
        ILabelRepository labelRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteLabelCommandHandler> logger)
    {
        _labelRepository = labelRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteLabelCommand command, CancellationToken cancellationToken)
    {
        var label = await _labelRepository.GetByIdAsync(command.Id, cancellationToken);
        if (label is null)
            return Result.Failure(LabelErrors.NotFound);

        var removable = label.EnsureRemovable();
        if (removable.IsFailure)
        {
            // A system configuration (066 FR-006).
            _logger.LogWarning(
                "Label {LabelId} deletion refused for tenant {TenantId}: {ErrorCode}.",
                label.Id, _tenantContext.TenantId, removable.Error.Code);
            return removable;
        }

        _labelRepository.Remove(label);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Label {LabelId} of tenant {TenantId} deleted with its format {LabelFormatId}; was default: {WasDefault}.",
            label.Id, _tenantContext.TenantId, label.LabelFormatId, label.IsDefault);

        return Result.Success();
    }
}
