using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Commands.CloseCashShift;

public sealed class CloseCashShiftCommandHandler
    : IRequestHandler<CloseCashShiftCommand, Result<CloseCashShiftResponse>>
{
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CloseCashShiftCommandHandler(
        ICashShiftRepository cashShiftRepository,
        ISaleRepository saleRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _cashShiftRepository = cashShiftRepository;
        _saleRepository = saleRepository;
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

        var totals = await _saleRepository.GetShiftSalesTotalsAsync(shift.Id, cancellationToken);

        var closeResult = shift.Close(command.ActualEndingCash, totals.CashSales);
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
