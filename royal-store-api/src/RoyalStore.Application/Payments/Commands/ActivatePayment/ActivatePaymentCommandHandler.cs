using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Payments.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Payments.Commands.ActivatePayment;

public sealed class ActivatePaymentCommandHandler
    : IRequestHandler<ActivatePaymentCommand, Result<PaymentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public ActivatePaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PaymentResponse>> Handle(
        ActivatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(command.Id, cancellationToken);
        if (payment is null)
            return Error.NotFound("payment.not_found", "Payment method not found.");

        var wasActive = payment.Active;

        var activateResult = payment.Activate();
        if (activateResult.IsFailure)
            return activateResult.Error;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!wasActive)
        {
            var userId = _currentUserService.CurrentUserId ?? "unknown";
            await _auditService.RecordAsync(
                "payment.activated",
                _tenantContext.TenantId,
                $"Payment method '{payment.Id}' activated.",
                userId,
                cancellationToken);
        }

        return PaymentResponse.From(payment);
    }
}
