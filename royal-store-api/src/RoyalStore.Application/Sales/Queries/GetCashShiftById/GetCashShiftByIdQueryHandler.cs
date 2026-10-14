using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetCashShiftById;

public sealed class GetCashShiftByIdQueryHandler
    : IRequestHandler<GetCashShiftByIdQuery, Result<CashShiftDetailsResponse>>
{
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly ISaleRepository _saleRepository;

    public GetCashShiftByIdQueryHandler(
        ICashShiftRepository cashShiftRepository,
        ISaleRepository saleRepository)
    {
        _cashShiftRepository = cashShiftRepository;
        _saleRepository = saleRepository;
    }

    public async Task<Result<CashShiftDetailsResponse>> Handle(
        GetCashShiftByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Any shift of the caller's store, open or closed, regardless of owner. The tenant query
        // filter makes another store's shift indistinguishable from a missing one.
        var result = await _cashShiftRepository.GetByIdWithOperatorNameAsync(request.CashShiftId, cancellationToken);
        if (result is null)
            return Error.NotFound("cashshift.not_found", "Cash shift not found.");

        var totals = await _saleRepository.GetShiftSalesTotalsAsync(result.Shift.Id, cancellationToken);

        return CashShiftDetailsResponse.From(result.Shift, result.OperatorName, totals);
    }
}
