using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Queries.GetCashShifts;

public sealed class GetCashShiftsQueryHandler
    : IRequestHandler<GetCashShiftsQuery, Result<IReadOnlyList<CashShiftListItemResponse>>>
{
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly ISaleRepository _saleRepository;

    public GetCashShiftsQueryHandler(
        ICashShiftRepository cashShiftRepository,
        ISaleRepository saleRepository)
    {
        _cashShiftRepository = cashShiftRepository;
        _saleRepository = saleRepository;
    }

    public async Task<Result<IReadOnlyList<CashShiftListItemResponse>>> Handle(
        GetCashShiftsQuery request,
        CancellationToken cancellationToken)
    {
        // Two tenant-wide reads merged here, so the list costs the same number of queries for any shift count.
        var shifts = await _cashShiftRepository.GetAllWithMovementTotalsAsync(cancellationToken);
        var totalsByShift = await _saleRepository.GetSalesTotalsByShiftAsync(cancellationToken);

        IReadOnlyList<CashShiftListItemResponse> response = shifts
            .Select(s => CashShiftListItemResponse.From(s, totalsByShift.GetValueOrDefault(s.Id, ShiftSalesTotals.Empty)))
            .ToList();

        return Result<IReadOnlyList<CashShiftListItemResponse>>.Success(response);
    }
}
