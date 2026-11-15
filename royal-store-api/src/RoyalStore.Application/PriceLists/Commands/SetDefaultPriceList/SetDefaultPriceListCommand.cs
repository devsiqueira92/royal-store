using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.SetDefaultPriceList;

/// <summary>
/// Makes a price list the store's default. Sales charge the default-list price, so this re-prices
/// the whole store: it is refused while any product lacks a price on the target list (061 FR-005).
/// </summary>
public record SetDefaultPriceListCommand(Guid Id) : IRequest<Result<PriceListResponse>>;

public sealed class SetDefaultPriceListCommandHandler
    : IRequestHandler<SetDefaultPriceListCommand, Result<PriceListResponse>>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniqueConstraintDetector _uniqueConstraintDetector;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SetDefaultPriceListCommandHandler> _logger;

    public SetDefaultPriceListCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork,
        IUniqueConstraintDetector uniqueConstraintDetector,
        ITenantContext tenantContext,
        ILogger<SetDefaultPriceListCommandHandler> logger)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
        _uniqueConstraintDetector = uniqueConstraintDetector;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PriceListResponse>> Handle(
        SetDefaultPriceListCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // One transaction: the swap and the re-mirroring of every product's legacy price either
            // all happen or none do. A failed Result rolls everything back.
            var result = await _unitOfWork.ExecuteInTransactionAsync(
                ct => SwitchAsync(command.Id, ct), cancellationToken);

            if (result.IsFailure)
                _logger.LogWarning(
                    "Default price list switch refused for tenant {TenantId}, target {PriceListId}: {ErrorCode}.",
                    _tenantContext.TenantId, command.Id, result.Error.Code);

            return result;
        }
        catch (DbUpdateException ex) when (_uniqueConstraintDetector.IsUniqueViolation(ex))
        {
            // Two switches at once: the "one default per store" index let exactly one through.
            _logger.LogWarning(
                "Default price list switch lost a concurrent race for tenant {TenantId}, target {PriceListId}.",
                _tenantContext.TenantId, command.Id);

            return Error.Conflict("pricelist.default_conflict",
                "The default price list was changed by someone else at the same time. Reload and try again.");
        }
    }

    private async Task<Result<PriceListResponse>> SwitchAsync(Guid targetId, CancellationToken ct)
    {
        var target = await _priceListRepository.GetByIdAsync(targetId, ct);
        if (target is null)
            return Error.NotFound("pricelist.not_found", "Price list was not found.");

        var counts = await _priceListRepository.GetProductCountsAsync(ct);

        if (target.IsDefault)
            return PriceListResponse.From(target, counts.GetValueOrDefault(target.Id));

        var productsWithoutPrice = await _priceListRepository.CountProductsWithoutPriceAsync(target.Id, ct);

        var allowed = target.CanBecomeDefault(productsWithoutPrice);
        if (allowed.IsFailure)
            return allowed.Error;

        // Two ordered saves, not one. "At most one default per store" is a unique index, which the
        // database checks statement by statement: marking the new list before the old one is
        // unmarked would trip it. Saving the unmark first removes any dependence on the order in
        // which a single save happens to emit its statements.
        var current = await _priceListRepository.GetDefaultAsync(ct);
        if (current is not null)
        {
            current.UnmarkDefault();
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var marked = target.MarkAsDefault(productsWithoutPrice);
        if (marked.IsFailure)
            return marked.Error;

        await _unitOfWork.SaveChangesAsync(ct);

        // The legacy single price — still what sales, the POS and the offline catalog read — must
        // now mean "the price on the new default list" (FR-020). A product that turns out to have
        // no price there (created while this was running) fails the whole switch.
        var products = await _priceListRepository.GetProductsWithPriceOnAsync(target.Id, ct);
        foreach (var product in products)
        {
            var mirrored = product.MirrorDefaultPrice(target);
            if (mirrored.IsFailure)
                return mirrored.Error;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Default price list for tenant {TenantId} is now {PriceListId}; {ProductsRepriced} products re-mirrored.",
            _tenantContext.TenantId, target.Id, products.Count);

        return PriceListResponse.From(target, counts.GetValueOrDefault(target.Id));
    }
}
