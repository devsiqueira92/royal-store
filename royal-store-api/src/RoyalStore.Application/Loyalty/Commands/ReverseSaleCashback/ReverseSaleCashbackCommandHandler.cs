using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Loyalty;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Commands.ReverseSaleCashback;

public sealed class ReverseSaleCashbackCommandHandler
    : IRequestHandler<ReverseSaleCashbackCommand, Result<ReverseSaleCashbackResponse>>
{
    private readonly ICashbackLedger _cashbackLedger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public ReverseSaleCashbackCommandHandler(
        ICashbackLedger cashbackLedger,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _cashbackLedger = cashbackLedger;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ReverseSaleCashbackResponse>> Handle(
        ReverseSaleCashbackCommand command,
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // 404 (unknown/unidentified sale) and 409 (already reversed, FR-022f) are both surfaced by
        // the ledger itself — see ICashbackLedger.ReverseForSaleAsync.
        var reversalResult = await _cashbackLedger.ReverseForSaleAsync(tenantId, command.SaleId, cancellationToken);
        if (reversalResult.IsFailure)
            return reversalResult.Error;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var outcome = reversalResult.Value;

        var userId = _currentUserService.CurrentUserId ?? "unknown";
        await _auditService.RecordAsync(
            "sale.cashback_reversed",
            tenantId,
            $"Sale '{command.SaleId}' cashback reversed. Clawed back: {outcome.ClawedBack}. Restored: {outcome.RestoredToCustomer}.",
            userId,
            cancellationToken);

        return new ReverseSaleCashbackResponse(outcome.SaleId, outcome.ClawedBack, outcome.RestoredToCustomer);
    }
}
