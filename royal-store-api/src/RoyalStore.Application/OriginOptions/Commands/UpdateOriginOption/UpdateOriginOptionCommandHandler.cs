using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.OriginOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.OriginOptions.Commands.UpdateOriginOption;

public sealed class UpdateOriginOptionCommandHandler
    : IRequestHandler<UpdateOriginOptionCommand, Result<OriginOptionResponse>>
{
    private readonly IOriginOptionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateOriginOptionCommandHandler(
        IOriginOptionRepository repository,
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

    public async Task<Result<OriginOptionResponse>> Handle(
        UpdateOriginOptionCommand command,
        CancellationToken cancellationToken)
    {
        var option = await _repository.GetByCodeAsync(command.CurrentCode, cancellationToken);
        if (option is null)
            return Error.NotFound("origin_option.not_found", "Origin option not found.");

        var codeExists = await _repository.ExistsByCodeAsync(command.Code, option.Id, cancellationToken);
        if (codeExists)
            return Error.Conflict("origin_option.code_conflict", $"An origin option with code '{command.Code}' already exists.");

        var updateResult = option.Update(command.Code, command.Name, command.Description);
        if (!updateResult.IsSuccess)
            return updateResult.Error;

        await _repository.UpdateAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "origin_option.updated",
            _tenantContext.TenantId,
            $"Origin option '{option.Id}' updated by SuperAdmin. New code: {option.Code}.",
            userId,
            cancellationToken);

        return OriginOptionResponse.From(option);
    }
}
