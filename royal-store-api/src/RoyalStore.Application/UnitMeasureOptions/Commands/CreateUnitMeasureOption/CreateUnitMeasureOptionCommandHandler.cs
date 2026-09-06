using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.UnitMeasureOptions.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.UnitMeasureOptions.Commands.CreateUnitMeasureOption;

public sealed class CreateUnitMeasureOptionCommandHandler
    : IRequestHandler<CreateUnitMeasureOptionCommand, Result<UnitMeasureOptionResponse>>
{
    private readonly IUnitMeasureOptionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateUnitMeasureOptionCommandHandler(
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
        CreateUnitMeasureOptionCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        var codeExists = await _repository.ExistsByCodeAsync(command.Code, null, cancellationToken);
        if (codeExists)
            return Error.Conflict("unit_measure_option.code_conflict", $"A unit measure option with code '{command.Code}' already exists.");

        var optionResult = UnitMeasureOption.Create(tenantId, command.Code, command.Name, command.Description);
        if (!optionResult.IsSuccess)
            return optionResult.Error;

        var option = optionResult.Value;

        await _repository.AddAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "unit_measure_option.created",
            tenantId,
            $"Unit measure option '{option.Name}' created. Id: {option.Id}",
            userId,
            cancellationToken);

        return UnitMeasureOptionResponse.From(option);
    }
}
