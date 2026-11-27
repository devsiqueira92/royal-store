using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.CostCompositions.Commands.DeleteCostComposition;

/// <summary>
/// Deletes a list and its lines (064 FR-012). A list in use may be deleted: every product that used
/// it simply shows none afterwards — the database clears their reference, so no product is loaded.
/// </summary>
public record DeleteCostCompositionCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteCostCompositionCommandHandler
    : IRequestHandler<DeleteCostCompositionCommand, Result>
{
    private readonly ICostCompositionRepository _costCompositionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<DeleteCostCompositionCommandHandler> _logger;

    public DeleteCostCompositionCommandHandler(
        ICostCompositionRepository costCompositionRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<DeleteCostCompositionCommandHandler> logger)
    {
        _costCompositionRepository = costCompositionRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result> Handle(
        DeleteCostCompositionCommand command, CancellationToken cancellationToken)
    {
        var list = await _costCompositionRepository.GetByIdAsync(command.Id, cancellationToken);
        if (list is null)
            return Result.Failure(Error.NotFound("costcomposition.not_found", "Cost composition was not found."));

        // Products that used the list are left without one (FK set to null by the database).
        _costCompositionRepository.Remove(list);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cost composition {CostCompositionId} of tenant {TenantId} deleted; products using it now have none.",
            command.Id, _tenantContext.TenantId);

        return Result.Success();
    }
}
