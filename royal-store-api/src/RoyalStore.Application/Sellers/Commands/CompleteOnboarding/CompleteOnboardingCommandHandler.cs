using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Sellers.Commands.CompleteOnboarding;

public sealed class CompleteOnboardingCommandHandler
    : IRequestHandler<CompleteOnboardingCommand, Result<SellerResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ISellerRepository _sellerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CompleteOnboardingCommandHandler(
        IIdentityService identityService,
        ISellerRepository sellerRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _sellerRepository = sellerRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<SellerResponse>> Handle(
        CompleteOnboardingCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Error.Unauthorized("auth.unauthenticated", "Not authenticated.");

        var user = await _identityService.FindByIdAsync(userId);
        if (user is null)
            return Error.Unauthorized("auth.unauthenticated", "Not authenticated.");

        var seller = await _sellerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (seller is null)
            return Error.NotFound("seller.not_found", "Seller not found.");

        if (!seller.IsActive)
            return Error.Validation("seller.inactive", "Cannot complete onboarding for an inactive seller.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(seller.Cpf))
        {
            var normalizedCpf = CpfValidator.Normalize(command.Cpf ?? string.Empty);
            if (await _sellerRepository.ExistsByCpfAsync(normalizedCpf, seller.Id, cancellationToken))
                return Error.Conflict("seller.cpf_already_exists", $"A seller with CPF '{normalizedCpf}' already exists.");
        }

        if (await _sellerRepository.ExistsByEmailAsync(normalizedEmail, seller.Id, cancellationToken))
            return Error.Conflict("seller.email_already_exists", $"A seller with email '{normalizedEmail}' already exists.");

        var mustChangePassword = user.RequiresPasswordChange;
        var willChangePassword = !string.IsNullOrWhiteSpace(command.NewPassword);

        if (mustChangePassword && !willChangePassword)
            return Error.Validation("seller.password_required", "A new password is required to complete onboarding.");

        return await _unitOfWork.ExecuteInTransactionAsync<SellerResponse>(async ct =>
        {
            if (willChangePassword)
            {
                var passwordResult = await _identityService.ChangePasswordAsync(
                    user, command.CurrentPassword, command.NewPassword);
                if (!passwordResult.IsSuccess)
                    return passwordResult.Error;
            }

            var onboardingResult = seller.CompleteOnboarding(
                command.FullName, command.Phone, command.Cpf, normalizedEmail);
            if (!onboardingResult.IsSuccess)
                return onboardingResult.Error;

            if (willChangePassword)
            {
                user.MarkPasswordChanged();
                var clearResult = await _identityService.ClearPasswordChangeFlagAsync(user);
                if (!clearResult.IsSuccess)
                    return clearResult.Error;
            }

            await _sellerRepository.UpdateAsync(seller, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _auditService.RecordAsync(
                "seller.onboarding_completed",
                _tenantContext.TenantId,
                string.Empty,
                seller.Cpf ?? string.Empty,
                ct);

            return SellerResponse.From(seller);
        }, cancellationToken);
    }
}
