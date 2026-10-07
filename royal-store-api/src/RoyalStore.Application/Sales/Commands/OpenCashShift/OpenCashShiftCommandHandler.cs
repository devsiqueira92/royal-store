using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.OpenCashShift;

public sealed class OpenCashShiftCommandHandler
    : IRequestHandler<OpenCashShiftCommand, Result<OpenCashShiftResponse>>
{
    private readonly ICashShiftRepository _cashShiftRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public OpenCashShiftCommandHandler(
        ICashShiftRepository cashShiftRepository,
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _cashShiftRepository = cashShiftRepository;
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<OpenCashShiftResponse>> Handle(
        OpenCashShiftCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId;
        var tenantId = _tenantContext.TenantId;

        if (userId is not null)
        {
            var existing = await _cashShiftRepository.GetOpenByUserIdAsync(userId, cancellationToken);
            if (existing is not null)
                return Error.Conflict("cashshift.already_open", "A cash shift is already open for this user.");
        }

        var shiftResult = CashShift.Create(Guid.NewGuid(), tenantId, userId, DateTime.UtcNow, command.openingBalance);
        if (!shiftResult.IsSuccess)
            return shiftResult.Error;

        var shift = shiftResult.Value;

        await _cashShiftRepository.AddAsync(shift, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new OpenCashShiftResponse(shift.Id);
    }
}
