using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Payments.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Payments.Commands.DeactivatePayment;

public sealed class DeactivatePaymentCommandHandler
    : IRequestHandler<DeactivatePaymentCommand, Result<PaymentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeactivatePaymentCommandHandler(
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
        DeactivatePaymentCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(command.Id, cancellationToken);
        if (payment is null)
            return Error.NotFound("payment.not_found", "Payment method not found.");

        // Only a genuine active -> inactive transition can strand the tenant with zero active
        // methods (FR-012); deactivating an already-inactive one is a harmless no-op below.
        if (payment.Active)
        {
            var activeCount = await _paymentRepository.CountActiveAsync(cancellationToken);
            if (activeCount <= 1)
                return Error.Conflict("payment.last_active_method",
                    "This is the tenant's last active payment method and cannot be deactivated. Activate another method first.");
        }

        var wasActive = payment.Active;

        var deactivateResult = payment.Deactivate();
        if (deactivateResult.IsFailure)
            return deactivateResult.Error;

        await _paymentRepository.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (wasActive)
        {
            var userId = _currentUserService.CurrentUserId ?? "unknown";
            await _auditService.RecordAsync(
                "payment.deactivated",
                _tenantContext.TenantId,
                $"Payment method '{payment.Id}' deactivated.",
                userId,
                cancellationToken);
        }

        return PaymentResponse.From(payment);
    }
}
