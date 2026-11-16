using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.SetPriceListRule;

/// <summary>
/// Gives a price list a percentage rule, or changes the one it has (062): "this list is
/// <paramref name="BasePriceListId"/> plus <paramref name="DiscountPercentage"/> percent".
/// The rule only feeds suggestions — no stored product price is touched.
/// </summary>
/// <param name="DiscountPercentage">Negative = discount, positive = surcharge.</param>
public record SetPriceListRuleCommand(
    Guid Id,
    Guid? BasePriceListId,
    decimal? DiscountPercentage) : IRequest<Result<PriceListResponse>>;

public sealed class SetPriceListRuleValidator : AbstractValidator<SetPriceListRuleCommand>
{
    public SetPriceListRuleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class SetPriceListRuleCommandHandler
    : IRequestHandler<SetPriceListRuleCommand, Result<PriceListResponse>>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<SetPriceListRuleCommandHandler> _logger;

    public SetPriceListRuleCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ILogger<SetPriceListRuleCommandHandler> logger)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<Result<PriceListResponse>> Handle(
        SetPriceListRuleCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.Id, cancellationToken);
        if (priceList is null)
            return Error.NotFound("pricelist.not_found", "Price list was not found.");

        // Removing a rule is its own request; this one always carries a whole rule. Checked here
        // rather than in the validator: the validation pipeline reports every failure as
        // "validation.failed", and this one has a code of its own in the contract.
        if (command.BasePriceListId is not { } basePriceListId || command.DiscountPercentage is not { } percentage)
            return Refused(command, Error.Validation("pricelist.rule_incomplete",
                "A rule needs both the base price list and the percentage."));

        // The tenant filter makes another store's list come back as null here.
        var baseList = await _priceListRepository.GetByIdAsync(basePriceListId, cancellationToken);
        if (baseList is null)
            return Refused(command, Error.Validation("pricelist.base_not_found", "The base price list was not found."));

        var listsBasedOnThis = await _priceListRepository.CountListsBasedOnAsync(priceList.Id, cancellationToken);

        var ruled = priceList.SetRule(baseList, percentage, listsBasedOnThis);
        if (ruled.IsFailure)
            return Refused(command, ruled.Error);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Price list {PriceListId} of tenant {TenantId} now follows {BasePriceListId} at {DiscountPercentage}%.",
            priceList.Id, _tenantContext.TenantId, baseList.Id, priceList.DiscountPercentage);

        var counts = await _priceListRepository.GetProductCountsAsync(cancellationToken);
        var storeLists = await _priceListRepository.GetAllAsync(null, cancellationToken);
        return PriceListResponse.From(priceList, counts.GetValueOrDefault(priceList.Id), storeLists);
    }

    private Error Refused(SetPriceListRuleCommand command, Error error)
    {
        _logger.LogWarning(
            "Rule refused for price list {PriceListId} of tenant {TenantId}: {ErrorCode}.",
            command.Id, _tenantContext.TenantId, error.Code);

        return error;
    }
}
