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

namespace RoyalStore.Application.Tenants.Commands.ResendConfirmationEmail;

public sealed class ResendConfirmationEmailCommandHandler(
    IIdentityService identityService,
    IEmailService emailService,
    IConfirmationResendRequestRepository confirmationResendRequestRepository,
    ISecurityAuditService securityAuditService,
    IUnitOfWork unitOfWork,
    IOptions<ResendConfirmationOptions> options)
    : IRequestHandler<ResendConfirmationEmailCommand, Result>
{
    private readonly ResendConfirmationOptions _options = options.Value;

    public async Task<Result> Handle(ResendConfirmationEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailAsync(command.Email);

        if (user is null)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Success();

        var cooldownSince = DateTimeOffset.UtcNow.AddSeconds(-_options.CooldownSeconds);
        var cooldownCount = await confirmationResendRequestRepository.CountRecentAsync(
            user.Id, user.TenantId, cooldownSince, cancellationToken);

        if (cooldownCount >= 1)
        {
            await securityAuditService.RecordAsync(
                "confirmation.cooldown_active",
                user.TenantId,
                $"Resend declined: request already sent within the last {_options.CooldownSeconds} seconds.",
                ObfuscateEmail(command.Email),
                cancellationToken);

            return Result.Success();
        }

        var windowSince = DateTimeOffset.UtcNow.AddHours(-_options.WindowHours);
        var windowCount = await confirmationResendRequestRepository.CountRecentAsync(
            user.Id, user.TenantId, windowSince, cancellationToken);

        if (windowCount >= _options.MaxAttemptsPerWindow)
        {
            await securityAuditService.RecordAsync(
                "confirmation.daily_cap_reached",
                user.TenantId,
                $"Resend declined: {windowCount} attempts in the last {_options.WindowHours} hours.",
                ObfuscateEmail(command.Email),
                cancellationToken);

            return Result.Success();
        }

        var activeRequest = await confirmationResendRequestRepository.FindActiveByUserIdAsync(
            user.Id, user.TenantId, cancellationToken);

        activeRequest?.Consume();

        // Unconditional: the very first confirmation token is issued at registration time
        // (no ConfirmationResendRequest exists yet for it), so invalidation cannot be gated
        // on finding a prior tracked request — every resend must rotate the SecurityStamp.
        await identityService.UpdateSecurityStampAsync(user);
        user = await identityService.FindByEmailAsync(command.Email);
        if (user is null)
            return Result.Success();

        var token = await identityService.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        try
        {
            await emailService.SendEmailVerificationAsync(command.Email, user.Id, encodedToken, cancellationToken);
        }
        catch (Exception ex)
        {
            await securityAuditService.RecordAsync(
                "confirmation.send_failed",
                user.TenantId,
                ex.Message,
                ObfuscateEmail(command.Email),
                cancellationToken);

            return Result.Success();
        }

        var requestResult = ConfirmationResendRequest.Create(user.Id, user.TenantId);
        if (!requestResult.IsSuccess)
            return Result.Failure(requestResult.Error);

        await confirmationResendRequestRepository.AddAsync(requestResult.Value, cancellationToken);
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
