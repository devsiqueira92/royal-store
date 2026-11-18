using MediatR;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.CostCompositions.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.CostCompositions.Commands.CreateCostComposition;

public sealed class CreateCostCompositionCommandHandler
    : IRequestHandler<CreateCostCompositionCommand, Result<CostCompositionResponse>>
{
    private readonly ICostCompositionRepository _costCompositionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<CreateCostCompositionCommandHandler> _logger;

    public CreateCostCompositionCommandHandler(
        ICostCompositionRepository costCompositionRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<CreateCostCompositionCommandHandler> logger)
    {
        _costCompositionRepository = costCompositionRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<CostCompositionResponse>> Handle(
        CreateCostCompositionCommand command, CancellationToken cancellationToken)
    {
        var created = CostCompositionList.Create(_tenantContext.TenantId, command.Name, command.Items);
        if (created.IsFailure)
            return created.Error;

        var list = created.Value;

        // The lines are final, so the total can be taken now.
        list.RecalculateTotal();

        await _costCompositionRepository.AddAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cost composition {CostCompositionId} created for tenant {TenantId} with {ItemCount} items.",
            list.Id, _tenantContext.TenantId, list.Items.Count);

        return CostCompositionResponse.From(list);
    }
}
