using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Commands.SetPriceListActive;


public sealed class SetPriceListActiveCommandHandler
    : IRequestHandler<SetPriceListActiveCommand, Result>
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPriceListActiveCommandHandler(
        IPriceListRepository priceListRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SetPriceListActiveCommand command, CancellationToken cancellationToken)
    {
        var priceList = await _priceListRepository.GetByIdAsync(command.Id, cancellationToken);
        if (priceList is null)
            return Result.Failure(Error.NotFound("pricelist.not_found", "Price list was not found."));

        // A list other lists are calculated from cannot be switched off from under them (062 FR-009).
        var transition = command.IsActive
            ? priceList.Activate()
            : priceList.Deactivate(
                await _priceListRepository.CountListsBasedOnAsync(priceList.Id, cancellationToken));
        if (transition.IsFailure)
            return transition;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
