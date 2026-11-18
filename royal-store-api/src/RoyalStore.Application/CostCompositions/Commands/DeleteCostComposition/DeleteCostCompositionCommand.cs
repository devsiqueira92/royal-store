using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.CostCompositions.Commands.DeleteCostComposition;

/// <summary>
/// Deletes a list and its lines (064 FR-012). A list linked to a product may be deleted: it is that
/// product's composition and nothing else depends on it, so the product simply shows none afterwards.
/// The product record is not loaded or changed (research D10).
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

        var productId = list.ProductId;

        _costCompositionRepository.Remove(list);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cost composition {CostCompositionId} of tenant {TenantId} deleted; it was linked to product {ProductId}.",
            command.Id, _tenantContext.TenantId, productId);

        return Result.Success();
    }
}
