using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.DeletePriceList;

/// <summary>Deletes a price list that is neither the default nor priced on any product (061 FR-006).</summary>
public record DeletePriceListCommand(Guid Id) : IRequest<Result>;

public sealed class DeletePriceListCommandHandler
    : IRequestHandler<DeletePriceListCommand, Result>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePriceListCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeletePriceListCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.Id, cancellationToken);
        if (priceList is null)
            return Result.Failure(Error.NotFound("pricelist.not_found", "Price list was not found."));

        var hasProductPrices = await _priceListRepository.HasProductPricesAsync(priceList.Id, cancellationToken);

        var listsBasedOnThis = await _priceListRepository.CountListsBasedOnAsync(priceList.Id, cancellationToken);

        var deletable = priceList.EnsureDeletable(hasProductPrices, listsBasedOnThis);
        if (deletable.IsFailure)
            return deletable;

        _priceListRepository.Remove(priceList);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
