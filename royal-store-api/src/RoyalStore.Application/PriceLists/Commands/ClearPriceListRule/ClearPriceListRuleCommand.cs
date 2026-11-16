using MediatR;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.ClearPriceListRule;

/// <summary>
/// Removes a price list's percentage rule (062): the list goes back to being priced by hand.
/// Stored product prices stay exactly as they are. A list without a rule is returned unchanged.
/// </summary>
public record ClearPriceListRuleCommand(Guid Id) : IRequest<Result<PriceListResponse>>;

public sealed class ClearPriceListRuleCommandHandler
    : IRequestHandler<ClearPriceListRuleCommand, Result<PriceListResponse>>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<ClearPriceListRuleCommandHandler> _logger;

    public ClearPriceListRuleCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<ClearPriceListRuleCommandHandler> logger)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PriceListResponse>> Handle(
        ClearPriceListRuleCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.Id, cancellationToken);
        if (priceList is null)
            return Error.NotFound("pricelist.not_found", "Price list was not found.");

        if (priceList.HasRule)
        {
            priceList.ClearRule();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Price list {PriceListId} of tenant {TenantId} no longer has a percentage rule.",
                priceList.Id, _tenantContext.TenantId);
        }

        var counts = await _priceListRepository.GetProductCountsAsync(cancellationToken);
        var storeLists = await _priceListRepository.GetAllAsync(null, cancellationToken);
        return PriceListResponse.From(priceList, counts.GetValueOrDefault(priceList.Id), storeLists);
    }
}
