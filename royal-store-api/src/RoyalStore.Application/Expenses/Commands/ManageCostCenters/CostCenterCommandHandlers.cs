using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Expenses.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Expenses;

namespace RoyalStore.Application.Expenses.Commands.ManageCostCenters;

public sealed class CreateCostCenterCommandHandler
    : IRequestHandler<CreateCostCenterCommand, Result<CostCenterResponse>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCostCenterCommandHandler(
        ICostCenterRepository costCenterRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CostCenterResponse>> Handle(
        CreateCostCenterCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.Code) &&
            await _costCenterRepository.ExistsByCodeAsync(command.Code.Trim(), null, cancellationToken))
        {
            return Error.Conflict("costcenter.duplicate_code",
                "A cost center with this code already exists.");
        }

        var result = CostCenter.Create(_tenantContext.TenantId, command.Name, command.Code);
        if (result.IsFailure)
            return result.Error;

        await _costCenterRepository.AddAsync(result.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CostCenterResponse.From(result.Value);
    }
}

public sealed class UpdateCostCenterCommandHandler
    : IRequestHandler<UpdateCostCenterCommand, Result<CostCenterResponse>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCostCenterCommandHandler(
        ICostCenterRepository costCenterRepository,
        IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CostCenterResponse>> Handle(
        UpdateCostCenterCommand command, CancellationToken cancellationToken)
    {
        var costCenter = await _costCenterRepository.GetByIdAsync(command.Id, cancellationToken);
        if (costCenter is null)
            return Error.NotFound("costcenter.not_found", "Cost center was not found.");

        if (!string.IsNullOrWhiteSpace(command.Code) &&
            await _costCenterRepository.ExistsByCodeAsync(command.Code.Trim(), command.Id, cancellationToken))
        {
            return Error.Conflict("costcenter.duplicate_code",
                "A cost center with this code already exists.");
        }

        var update = costCenter.Update(command.Name, command.Code);
        if (update.IsFailure)
            return update.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CostCenterResponse.From(costCenter);
    }
}

public sealed class SetCostCenterActiveCommandHandler
    : IRequestHandler<SetCostCenterActiveCommand, Result<CostCenterResponse>>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetCostCenterActiveCommandHandler(
        ICostCenterRepository costCenterRepository,
        IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CostCenterResponse>> Handle(
        SetCostCenterActiveCommand command, CancellationToken cancellationToken)
    {
        var costCenter = await _costCenterRepository.GetByIdAsync(command.Id, cancellationToken);
        if (costCenter is null)
            return Error.NotFound("costcenter.not_found", "Cost center was not found.");

        var transition = command.IsActive ? costCenter.Activate() : costCenter.Deactivate();
        if (transition.IsFailure)
            return transition.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CostCenterResponse.From(costCenter);
    }
}

public sealed class DeleteCostCenterCommandHandler
    : IRequestHandler<DeleteCostCenterCommand, Result>
{
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCostCenterCommandHandler(
        ICostCenterRepository costCenterRepository,
        IUnitOfWork unitOfWork)
    {
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteCostCenterCommand command, CancellationToken cancellationToken)
    {
        var costCenter = await _costCenterRepository.GetByIdAsync(command.Id, cancellationToken);
        if (costCenter is null)
            return Result.Failure(Error.NotFound("costcenter.not_found", "Cost center was not found."));

        if (await _costCenterRepository.IsReferencedAsync(command.Id, cancellationToken))
            return Result.Failure(Error.Conflict("costcenter.in_use",
                "This cost center is in use by an expense or a template. Deactivate it instead."));

        await _costCenterRepository.RemoveAsync(costCenter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
