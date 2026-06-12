using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Email;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Identity;
using RoyalStore.Domain.Tenants;
using System.Text;

namespace RoyalStore.Application.Tenants.Commands.RegisterTenant;

public sealed class RegisterTenantCommandHandler
    : IRequestHandler<RegisterTenantCommand, Result<RegistrationResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public RegisterTenantCommandHandler(
        IIdentityService identityService,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _identityService = identityService;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
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
                await _unitOfWork.SaveChangesAsync(ct);

                var token = await _identityService.GenerateEmailConfirmationTokenAsync(createdUser);
                capturedEncodedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                capturedUserId = createdUser.Id;

                return Result<RegistrationResponse>.Success(new RegistrationResponse(tenant.Id));
            },
            cancellationToken);

        if (!transactionResult.IsSuccess)
            return transactionResult.Error;

        await _emailService.SendEmailVerificationAsync(
            command.Email, capturedUserId!, capturedEncodedToken!, cancellationToken);

        return transactionResult;
    }

    private Task<Result<RegistrationResponse>> CreateTransaction(UserEntity user, RegisterTenantCommand command, Tenant tenant, string? capturedEncodedToken, string? capturedUserId, CancellationToken cancellationToken)
    {
        return _unitOfWork.ExecuteInTransactionAsync(
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
                await _unitOfWork.SaveChangesAsync(ct);

                var token = await _identityService.GenerateEmailConfirmationTokenAsync(createdUser);
                capturedEncodedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                capturedUserId = createdUser.Id;

                return Result<RegistrationResponse>.Success(new RegistrationResponse(tenant.Id));
            },
            cancellationToken);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
