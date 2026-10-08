using MediatR;
using Microsoft.EntityFrameworkCore;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.RecordCashMovement;

public sealed class RecordCashMovementCommandHandler
    : IRequestHandler<RecordCashMovementCommand, Result<CashMovementResponse>>
{
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public RecordCashMovementCommandHandler(
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

    public async Task<Result<CashMovementResponse>> Handle(
        RecordCashMovementCommand command,
        CancellationToken cancellationToken)
    {
        // The target shift is always the caller's own open shift — never a value from the request.
        var userId = _currentUserService.CurrentUserId ?? string.Empty;

        var shift = await _cashShiftRepository.GetOpenByUserIdAsync(userId, cancellationToken);
        if (shift is null)
            return Error.NotFound("cashshift.no_open_shift", "No active cash shift found for this user.");

        var movementId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        Result<CashMovement> movementResult;
        switch (command.Type)
        {
            case MovementType.Out:
                var totals = await _saleRepository.GetShiftSalesTotalsAsync(shift.Id, cancellationToken);
                movementResult = shift.RecordWithdrawal(
                    movementId, userId, command.Amount, command.Description, now, totals.CashSales);
                break;
            case MovementType.In:
                movementResult = shift.RecordSupplement(
                    movementId, userId, command.Amount, command.Description, now);
                break;
            default:
                return Error.Validation("cashmovement.type_invalid", "Movement type is invalid.");
        }

        if (movementResult.IsFailure)
            return movementResult.Error;

        // No UpdateAsync(shift): the shift is already tracked, so its Version bump is detected on save,
        // and Update() would mark the new movement Modified instead of Added.
        await _cashShiftRepository.AddMovementAsync(movementResult.Value, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another movement or a close committed against this shift after it was loaded; the
            // expected-cash check above may be stale, so nothing is recorded and the caller retries.
            return Error.Conflict("cashshift.concurrent_update",
                "The cash shift was changed by another operation. Refresh and try again.");
        }

        return CashMovementResponse.From(movementResult.Value);
    }
}
