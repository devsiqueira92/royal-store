using MediatR;
using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.UpdatePriceList;

public sealed class UpdatePriceListCommandHandler(
    IPriceListRepository priceListRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext,
    ILogger<UpdatePriceListCommandHandler> logger)
    : IRequestHandler<UpdatePriceListCommand, Result<PriceListResponse>>
{
    private static readonly Error DuplicateName = Error.Conflict(
        "pricelist.name_duplicate", "A price list with this name already exists.");

    public async Task<Result<PriceListResponse>> Handle(
        UpdatePriceListCommand command, CancellationToken cancellationToken)
    {
        // The tenant filter makes another store's list come back as null here.
        var priceList = await priceListRepository.GetByIdAsync(command.Id, cancellationToken);
        if (priceList is null)
            return Error.NotFound("pricelist.not_found", "Price list was not found.");

        var renamed = await RenameAsync(priceList, command.Name, cancellationToken);
        if (renamed.IsFailure)
            return Refused(command, renamed.Error);

        var ruled = await ApplyRuleAsync(priceList, command, cancellationToken);
        if (ruled.IsFailure)
            return Refused(command, ruled.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Price list {PriceListId} of tenant {TenantId} updated (base {BasePriceListId}, {DiscountPercentage}%).",
            priceList.Id, tenantContext.TenantId, priceList.BasePriceListId, priceList.DiscountPercentage);

        var counts = await priceListRepository.GetProductCountsAsync(cancellationToken);
        var storeLists = await priceListRepository.GetAllAsync(null, cancellationToken);
        return PriceListResponse.From(priceList, counts.GetValueOrDefault(priceList.Id), storeLists);
    }

    private async Task<Result> RenameAsync(PriceList priceList, string name, CancellationToken ct)
    {
        var normalized = PriceList.Normalize(name);
        if (normalized != priceList.NormalizedName
            && await priceListRepository.ExistsByNormalizedNameAsync(normalized, priceList.Id, ct))
            return Result.Failure(DuplicateName);

        return priceList.Rename(name);
    }

    private async Task<Result> ApplyRuleAsync(
        PriceList priceList, UpdatePriceListCommand command, CancellationToken ct)
    {
        // Checked here rather than in the validator: the validation pipeline reports every
        // failure as "validation.failed", and this one has a code of its own in the contract.
        if (command.BasePriceListId.HasValue != command.DiscountPercentage.HasValue)
            return Result.Failure(Error.Validation("pricelist.rule_incomplete",
                "A rule needs both the base price list and the percentage."));

        if (command.BasePriceListId is not { } basePriceListId || command.DiscountPercentage is not { } percentage)
        {
            priceList.ClearRule();
            return Result.Success();
        }

        // Unchanged rule: nothing to re-check, so a rename never fails on a rule saved earlier
        // (e.g. a base list deactivated since).
        if (priceList.BasePriceListId == basePriceListId && priceList.DiscountPercentage == percentage)
            return Result.Success();

        var baseList = await priceListRepository.GetByIdAsync(basePriceListId, ct);
        if (baseList is null)
            return Result.Failure(Error.Validation("pricelist.base_not_found", "The base price list was not found."));

        var listsBasedOnThis = await priceListRepository.CountListsBasedOnAsync(priceList.Id, ct);
        return priceList.SetRule(baseList, percentage, listsBasedOnThis);
    }

    private Error Refused(UpdatePriceListCommand command, Error error)
    {
        logger.LogWarning(
            "Update refused for price list {PriceListId} of tenant {TenantId}: {ErrorCode}.",
            command.Id, tenantContext.TenantId, error.Code);

        return error;
    }
}
