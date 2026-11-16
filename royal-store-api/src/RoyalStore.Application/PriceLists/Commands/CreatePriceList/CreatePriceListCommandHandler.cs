using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.CreatePriceList;

public sealed class CreatePriceListCommandHandler
    : IRequestHandler<CreatePriceListCommand, Result<PriceListResponse>>
{
    private static readonly Error DuplicateName = Error.Conflict(
        "pricelist.name_duplicate", "A price list with this name already exists.");

    private readonly IPriceListRepository _priceListRepository;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniqueConstraintDetector _uniqueConstraintDetector;

    public CreatePriceListCommandHandler(
        IPriceListRepository priceListRepository,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork,
        IUniqueConstraintDetector uniqueConstraintDetector)
    {
        _priceListRepository = priceListRepository;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
        _uniqueConstraintDetector = uniqueConstraintDetector;
    }

    public async Task<Result<PriceListResponse>> Handle(
        CreatePriceListCommand command, CancellationToken cancellationToken)
    {
        var created = PriceList.Create(_tenantContext.TenantId, command.Name);
        if (created.IsFailure)
            return created.Error;

        var priceList = created.Value;

        if (await _priceListRepository.ExistsByNormalizedNameAsync(
                priceList.NormalizedName, null, cancellationToken))
            return DuplicateName;

        // Checked here rather than in the validator: the validation pipeline reports every
        // failure as "validation.failed", and this one has a code of its own in the contract.
        if (command.BasePriceListId.HasValue != command.DiscountPercentage.HasValue)
            return Error.Validation("pricelist.rule_incomplete",
                "A rule needs both the base price list and the percentage.");

        if (command.BasePriceListId.HasValue && command.DiscountPercentage.HasValue)
        {
            var baseList = await _priceListRepository.GetByIdAsync(command.BasePriceListId.Value, cancellationToken);
            if (baseList is null)
                return Error.Validation("pricelist.base_not_found", "The base price list was not found.");

            // A list that does not exist yet cannot be anyone's base.
            var ruled = priceList.SetRule(baseList, command.DiscountPercentage.Value, listsBasedOnThis: 0);
            if (ruled.IsFailure)
                return ruled.Error;
        }

        await _priceListRepository.AddAsync(priceList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var storeLists = await _priceListRepository.GetAllAsync(null, cancellationToken);
        return PriceListResponse.From(priceList, productCount: 0, storeLists);
    }
}
