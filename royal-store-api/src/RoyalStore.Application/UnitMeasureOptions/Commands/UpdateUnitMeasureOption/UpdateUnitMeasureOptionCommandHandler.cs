using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.UnitMeasureOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.UnitMeasureOptions.Commands.UpdateUnitMeasureOption;

public sealed class UpdateUnitMeasureOptionCommandHandler
    : IRequestHandler<UpdateUnitMeasureOptionCommand, Result<UnitMeasureOptionResponse>>
{
    private readonly IUnitMeasureOptionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUnitMeasureOptionCommandHandler(
        IUnitMeasureOptionRepository repository,
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

    public async Task<Result<UnitMeasureOptionResponse>> Handle(
        UpdateUnitMeasureOptionCommand command,
        CancellationToken cancellationToken)
    {
        var option = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (option is null)
            return Error.NotFound("unit_measure_option.not_found", "Unit measure option not found.");

        var codeExists = await _repository.ExistsByCodeAsync(command.Code, option.Id, cancellationToken);
        if (codeExists)
            return Error.Conflict("unit_measure_option.code_conflict", $"A unit measure option with code '{command.Code}' already exists.");

        var updateResult = option.Update(command.Code, command.Name, command.Description);
        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _repository.UpdateAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "unit_measure_option.updated",
            _tenantContext.TenantId,
            $"Unit measure option '{option.Id}' updated.",
            userId,
            cancellationToken);

        return UnitMeasureOptionResponse.From(option);
    }
}
