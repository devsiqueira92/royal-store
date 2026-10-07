using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetCurrentCashShift;

public sealed class GetCurrentCashShiftQueryHandler
    : IRequestHandler<GetCurrentCashShiftQuery, Result<CashShiftResponse?>>
{
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentCashShiftQueryHandler(
        ICashShiftRepository cashShiftRepository,
        ISaleRepository saleRepository,
        ICurrentUserService currentUserService)
    {
        _cashShiftRepository = cashShiftRepository;
        _saleRepository = saleRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<CashShiftResponse?>> Handle(
        GetCurrentCashShiftQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId ?? string.Empty;

        var shift = await _cashShiftRepository.GetOpenByUserIdAsync(userId, cancellationToken);
        if (shift is null)
            return Result<CashShiftResponse?>.Success(null);

        var totals = await _saleRepository.GetShiftSalesTotalsAsync(shift.Id, cancellationToken);

        return Result<CashShiftResponse?>.Success(CashShiftResponse.From(shift, totals));
    }
}
