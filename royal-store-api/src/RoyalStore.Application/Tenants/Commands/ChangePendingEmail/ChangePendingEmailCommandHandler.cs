using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Options;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Email;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Application.Options;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Identity;

namespace RoyalStore.Application.Tenants.Commands.ChangePendingEmail;

public sealed class ChangePendingEmailCommandHandler(
    IIdentityService identityService,
    IEmailService emailService,
    IEmailChangeRequestRepository emailChangeRequestRepository,
    ISecurityAuditService securityAuditService,
    IUnitOfWork unitOfWork,
    IOptions<ChangePendingEmailOptions> options)
    : IRequestHandler<ChangePendingEmailCommand, Result>
{
    private readonly ChangePendingEmailOptions _options = options.Value;

    public async Task<Result> Handle(ChangePendingEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailAsync(command.CurrentEmail);

        if (user is not null)
        {
            var lockedSince = DateTimeOffset.UtcNow.AddMinutes(-_options.FailedAttemptsWindowMinutes);
            var lockedCount = await emailChangeRequestRepository.CountRecentAsync(
                user.Id, user.TenantId, wasSuccessful: false, lockedSince, cancellationToken);

            if (lockedCount >= _options.MaxFailedAttemptsPerWindow)
            {
                await securityAuditService.RecordAsync(
                    "email_change.locked_out",
                    user.TenantId,
                    $"Request declined: {lockedCount} failed attempts in the last {_options.FailedAttemptsWindowMinutes} minutes.",
                    ObfuscateEmail(command.CurrentEmail),
                    cancellationToken);

                return Result.Failure(Error.RateLimit("email_change.locked_out",
                    "Too many attempts. Please try again later."));
            }
        }

        var passwordValid = user is not null && await identityService.CheckPasswordAsync(user, command.Password);

        if (user is null || !passwordValid)
        {
            if (user is not null)
            {
                var failedRequestResult = EmailChangeRequest.Create(user.Id, user.TenantId, wasSuccessful: false);
                if (failedRequestResult.IsSuccess)
                    await emailChangeRequestRepository.AddAsync(failedRequestResult.Value, cancellationToken);

                await securityAuditService.RecordAsync(
                    "email_change.wrong_password",
                    user.TenantId,
                    "Change-pending-email request rejected: incorrect password.",
                    ObfuscateEmail(command.CurrentEmail),
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Failure(Error.Unauthorized("email_change.invalid_credentials",
                "Invalid email or password."));
        }

        if (user.EmailConfirmed)
            return Result.Failure(Error.Validation("email_change.already_confirmed",
                "This account is already active."));

        if (string.Equals(command.NewEmail, command.CurrentEmail, StringComparison.OrdinalIgnoreCase))
            return Result.Failure(Error.Validation("email_change.same_email",
                "The new email must be different from the current one."));

        var existing = await identityService.FindByEmailAsync(command.NewEmail);
        if (existing is not null)
            return Result.Failure(Error.Conflict("email_change.email_in_use",
                "This email address is already in use."));

        var cooldownSince = DateTimeOffset.UtcNow.AddSeconds(-_options.CooldownSeconds);
        var cooldownCount = await emailChangeRequestRepository.CountRecentAsync(
            user.Id, user.TenantId, wasSuccessful: true, cooldownSince, cancellationToken);

        if (cooldownCount >= 1)
        {
            await securityAuditService.RecordAsync(
                "email_change.cooldown_active",
                user.TenantId,
                $"Request declined: a successful change already happened within the last {_options.CooldownSeconds} seconds.",
                ObfuscateEmail(command.CurrentEmail),
                cancellationToken);

            return Result.Failure(Error.RateLimit("email_change.cooldown_active",
                "Please wait before requesting another change."));
        }

        var windowSince = DateTimeOffset.UtcNow.AddHours(-_options.SuccessfulChangesWindowHours);
        var windowCount = await emailChangeRequestRepository.CountRecentAsync(
            user.Id, user.TenantId, wasSuccessful: true, windowSince, cancellationToken);

        if (windowCount >= _options.MaxSuccessfulChangesPerWindow)
        {
            await securityAuditService.RecordAsync(
                "email_change.daily_cap_reached",
                user.TenantId,
                $"Request declined: {windowCount} successful changes in the last {_options.SuccessfulChangesWindowHours} hours.",
                ObfuscateEmail(command.CurrentEmail),
                cancellationToken);

            return Result.Failure(Error.RateLimit("email_change.daily_cap_reached",
                "Too many email changes. Please try again later."));
        }

        var changeResult = await identityService.ChangeEmailAsync(user, command.NewEmail);
        if (!changeResult.IsSuccess)
            return Result.Failure(changeResult.Error);

        var token = await identityService.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        try
        {
            await emailService.SendEmailVerificationAsync(command.NewEmail, user.Id, encodedToken, cancellationToken);
        }
        catch (Exception ex)
        {
            await securityAuditService.RecordAsync(
                "email_change.email_send_failed",
                user.TenantId,
                ex.Message,
                ObfuscateEmail(command.NewEmail),
                cancellationToken);
        }

        var requestResult = EmailChangeRequest.Create(user.Id, user.TenantId, wasSuccessful: true);
        if (!requestResult.IsSuccess)
            return Result.Failure(requestResult.Error);

        await emailChangeRequestRepository.AddAsync(requestResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string ObfuscateEmail(string email)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return Convert.ToHexString(hash)[..12];
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
