using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.RenamePriceList;

public record RenamePriceListCommand(Guid Id, string Name) : IRequest<Result<PriceListResponse>>;

public sealed class RenamePriceListValidator : AbstractValidator<RenamePriceListCommand>
{
    public RenamePriceListValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Price list name is required.")
            .MaximumLength(PriceList.NameMaxLength);
    }
}

public sealed class RenamePriceListCommandHandler
    : IRequestHandler<RenamePriceListCommand, Result<PriceListResponse>>
{
    private static readonly Error DuplicateName = Error.Conflict(
        "pricelist.name_duplicate", "A price list with this name already exists.");

    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUniqueConstraintDetector _uniqueConstraintDetector;

    public RenamePriceListCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork,
        IUniqueConstraintDetector uniqueConstraintDetector)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
        _uniqueConstraintDetector = uniqueConstraintDetector;
    }

    public async Task<Result<PriceListResponse>> Handle(
        RenamePriceListCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.Id, cancellationToken);
        if (priceList is null)
            return Error.NotFound("pricelist.not_found", "Price list was not found.");

        var renamed = priceList.Rename(command.Name);
        if (renamed.IsFailure)
            return renamed.Error;

        if (await _priceListRepository.ExistsByNormalizedNameAsync(
                priceList.NormalizedName, priceList.Id, cancellationToken))
            return DuplicateName;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (_uniqueConstraintDetector.IsUniqueViolation(ex))
        {
            return DuplicateName;
        }

        var counts = await _priceListRepository.GetProductCountsAsync(cancellationToken);
        var storeLists = await _priceListRepository.GetAllAsync(null, cancellationToken);
        return PriceListResponse.From(priceList, counts.GetValueOrDefault(priceList.Id), storeLists);
    }
}
