using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Profiles;
using RoyalStore.Domain.Sellers;

namespace RoyalStore.Application.Sellers.Commands.CompleteOnboarding;

public sealed class CompleteOnboardingCommandHandler
    : IRequestHandler<CompleteOnboardingCommand, Result<SellerResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ISellerRepository _sellerRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CompleteOnboardingCommandHandler(
        IIdentityService identityService,
        ISellerRepository sellerRepository,
        IProfileRepository profileRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _sellerRepository = sellerRepository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }
    /* TODO: Need to review this implemantation */
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

        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return Error.NotFound("profile.not_found", "Profile not found.");

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(profile.Cpf))
        {
            var normalizedCpf = CpfValidator.Normalize(command.Cpf ?? string.Empty);
            if (await _profileRepository.ExistsByCpfAsync(normalizedCpf, profile.Id, cancellationToken))
                return Error.Conflict("profile.cpf_already_exists", $"A profile with CPF '{normalizedCpf}' already exists.");
        }

        if (await _profileRepository.ExistsByEmailAsync(normalizedEmail, profile.Id, cancellationToken))
            return Error.Conflict("profile.email_already_exists", $"A profile with email '{normalizedEmail}' already exists.");

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

            var onboardingResult = profile.CompleteOnboarding(
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

            await _profileRepository.UpdateAsync(profile, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            await _auditService.RecordAsync(
                "seller.onboarding_completed",
                _tenantContext.TenantId,
                string.Empty,
                profile.Cpf ?? string.Empty,
                ct);

            return SellerResponse.From(seller, profile);
        }, cancellationToken);
    }
}
