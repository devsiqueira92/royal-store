using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetCashShiftMovements;

public sealed class GetCashShiftMovementsQueryHandler
    : IRequestHandler<GetCashShiftMovementsQuery, Result<IReadOnlyList<CashMovementResponse>>>
{
    private readonly ICashShiftRepository _cashShiftRepository;

    public GetCashShiftMovementsQueryHandler(ICashShiftRepository cashShiftRepository)
    {
        _cashShiftRepository = cashShiftRepository;
    }

    public async Task<Result<IReadOnlyList<CashMovementResponse>>> Handle(
        GetCashShiftMovementsQuery request,
        CancellationToken cancellationToken)
    {
        // Any shift of the caller's store, open or closed, regardless of owner. The tenant query
        // filter makes another store's shift indistinguishable from a missing one.
        var shift = await _cashShiftRepository.GetByIdWithMovementsAsync(request.CashShiftId, cancellationToken);
        if (shift is null)
            return Error.NotFound("cashshift.not_found", "Cash shift not found.");

        var movements = shift.Movements
            .OrderByDescending(m => m.MovedAt)
            .ThenBy(m => m.Id)
            .Select(CashMovementResponse.From)
            .ToList();

        return Result<IReadOnlyList<CashMovementResponse>>.Success(movements);
    }
}
