using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.CloseCashShift;

public sealed class CloseCashShiftCommandHandler
    : IRequestHandler<CloseCashShiftCommand, Result<CloseCashShiftResponse>>
{
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CloseCashShiftCommandHandler(
        ICashShiftRepository cashShiftRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _cashShiftRepository = cashShiftRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CloseCashShiftResponse>> Handle(
        CloseCashShiftCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId ?? string.Empty;

        var shift = await _cashShiftRepository.GetOpenByUserIdAsync(userId, cancellationToken);
        if (shift is null)
            return Error.NotFound("cashshift.no_open_shift", "No active cash shift found for this user.");

        var expectedEndingCash = shift.StartingCash
            + shift.Movements.Where(m => m.Type == MovementType.In).Sum(m => m.Amount)
            - shift.Movements.Where(m => m.Type == MovementType.Out).Sum(m => m.Amount);

        var closeResult = shift.Close(command.ActualEndingCash, expectedEndingCash);
        if (!closeResult.IsSuccess)
            return closeResult.Error;

        await _cashShiftRepository.UpdateAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CloseCashShiftResponse(
            shift.Id,
            shift.ClosedAt!.Value,
            shift.ExpectedEndingCash!.Value,
            shift.ActualEndingCash!.Value);
    }
}
