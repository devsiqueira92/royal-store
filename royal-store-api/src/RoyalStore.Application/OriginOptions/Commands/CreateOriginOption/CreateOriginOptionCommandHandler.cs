using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.OriginOptions.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.OriginOptions.Commands.CreateOriginOption;

public sealed class CreateOriginOptionCommandHandler
    : IRequestHandler<CreateOriginOptionCommand, Result<OriginOptionResponse>>
{
    private readonly IOriginOptionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateOriginOptionCommandHandler(
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
        CreateOriginOptionCommand command,
        CancellationToken cancellationToken)
    {
        var codeExists = await _repository.ExistsByCodeAsync(command.Code, null, cancellationToken);
        if (codeExists)
            return Error.Conflict("origin_option.code_conflict", $"An origin option with code '{command.Code}' already exists.");

        var optionResult = OriginOption.Create(command.Code, command.Name, command.Description);
        if (!optionResult.IsSuccess)
            return optionResult.Error;

        var option = optionResult.Value;

        await _repository.AddAsync(option, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // OriginOption is global (no TenantId) — the requester's own tenant is recorded here
        // only as audit metadata (who made the cross-tenant-affecting change), never for scoping.
        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "origin_option.created",
            _tenantContext.TenantId,
            $"Origin option '{option.Name}' (code {option.Code}) created by SuperAdmin. Id: {option.Id}",
            userId,
            cancellationToken);

        return OriginOptionResponse.From(option);
    }
}
