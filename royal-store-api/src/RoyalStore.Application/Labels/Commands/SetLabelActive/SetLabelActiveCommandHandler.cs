using MediatR;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Commands.SetLabelActive;

public sealed class SetLabelActiveCommandHandler : IRequestHandler<SetLabelActiveCommand, Result>
{
    private readonly ILabelRepository _labelRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SetLabelActiveCommandHandler> _logger;

    public SetLabelActiveCommandHandler(
        ILabelRepository labelRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<SetLabelActiveCommandHandler> logger)
    {
        _labelRepository = labelRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(SetLabelActiveCommand command, CancellationToken cancellationToken)
    {
        var label = await _labelRepository.GetByIdAsync(command.Id, cancellationToken);
        if (label is null)
            return Result.Failure(LabelErrors.NotFound);

        var wasDefault = label.IsDefault;

        if (command.IsActive)
            label.Activate();
        else
            label.Deactivate();

        // Repeating the current state changes nothing, so this save writes nothing.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Label {LabelId} of tenant {TenantId} set active: {IsActive}; default cleared: {DefaultCleared}.",
            label.Id, _tenantContext.TenantId, label.IsActive, wasDefault && !label.IsDefault);

        return Result.Success();
    }
}
