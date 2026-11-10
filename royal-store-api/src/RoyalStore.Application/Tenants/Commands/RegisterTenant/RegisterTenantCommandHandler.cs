using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Email;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Loyalty;
using RoyalStore.Domain.Payments;
using RoyalStore.Domain.Profiles;
using RoyalStore.Domain.Tenants;
using System.Security.Cryptography;
using System.Text;

namespace RoyalStore.Application.Tenants.Commands.RegisterTenant;

public sealed class RegisterTenantCommandHandler
    : IRequestHandler<RegisterTenantCommand, Result<RegistrationResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ISecurityAuditService _securityAuditService;

    public RegisterTenantCommandHandler(
        IIdentityService identityService,
        ITenantRepository tenantRepository,
        ILoyaltyTierRepository loyaltyTierRepository,
        IPaymentRepository paymentRepository,
        IProfileRepository profileRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ISecurityAuditService securityAuditService)
    {
        _identityService = identityService;
        _tenantRepository = tenantRepository;
        _loyaltyTierRepository = loyaltyTierRepository;
        _paymentRepository = paymentRepository;
        _profileRepository = profileRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _securityAuditService = securityAuditService;
    }

    public async Task<Result<RegistrationResponse>> Handle(
        RegisterTenantCommand command,
        CancellationToken cancellationToken)
    {
        var existingUser = await _identityService.FindByEmailAsync(command.Email);
        if (existingUser is not null)
            return Error.Conflict("registration.email_taken",
                "An account with this email address already exists.");

        var tenantResult = Tenant.Create(command.StoreName, command.Document, command.DocumentType);
        if (!tenantResult.IsSuccess)
            return tenantResult.Error;

        var tenant = tenantResult.Value;

        var documentExists = await _tenantRepository.ExistsAsync(tenant.Document, cancellationToken);
        if (documentExists)
            return Error.Conflict("registration.document_taken",
                "An account with this document is already registered.");

        var userResult = UserEntity.Create(command.Email, tenant.Id);
        if (!userResult.IsSuccess)
            return userResult.Error;

        var user = userResult.Value;

        string? capturedEncodedToken = null;
        string? capturedUserId = null;

        var transactionResult = await _unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                var createResult = await _identityService.CreateAsync(user, command.Password);
                if (!createResult.IsSuccess)
                    return (Result<RegistrationResponse>)createResult.Error;

                var createdUser = createResult.Value;

                var roleResult = await _identityService.AddToRoleAsync(createdUser, "Admin");
                if (!roleResult.IsSuccess)
                    return (Result<RegistrationResponse>)roleResult.Error;

                await _tenantRepository.AddAsync(tenant, ct);

                var profileResult = Profile.CreateStub(tenant.Id, createdUser.Id);
                if (!profileResult.IsSuccess)
                    return (Result<RegistrationResponse>)profileResult.Error;

                await _profileRepository.AddAsync(profileResult.Value, ct);

                // Every tenant starts with the four loyalty tiers (FR-010) — seeded here, in the same
                // transaction, so a tenant never exists without them (research.md Decision 9).
                await _loyaltyTierRepository.AddRangeAsync(LoyaltyTier.SeedDefaults(tenant.Id), ct);

                // Every tenant starts with its default payment methods catalog too (FR-003) — same
                // reasoning and same transaction as the loyalty tiers above (037 research.md Decision 12).
                await _paymentRepository.AddRangeAsync(Payment.SeedDefaults(tenant.Id), ct);

                await _unitOfWork.SaveChangesAsync(ct);

                var token = await _identityService.GenerateEmailConfirmationTokenAsync(createdUser);
                capturedEncodedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                capturedUserId = createdUser.Id;

                return Result<RegistrationResponse>.Success(new RegistrationResponse(tenant.Id, command.Email));
            },
            cancellationToken);

        if (!transactionResult.IsSuccess)
            return transactionResult.Error;

        try
        {
            await _emailService.SendEmailVerificationAsync(
                command.Email, capturedUserId!, capturedEncodedToken!, cancellationToken);
        }
        catch (Exception ex)
        {
            await _securityAuditService.RecordAsync(
                "registration.email_send_failed",
                tenant.Id,
                ex.Message,
                ObfuscateEmail(command.Email),
                cancellationToken);
        }

        return transactionResult;
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static string ObfuscateEmail(string email)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return Convert.ToHexString(hash)[..12];
    }
}
