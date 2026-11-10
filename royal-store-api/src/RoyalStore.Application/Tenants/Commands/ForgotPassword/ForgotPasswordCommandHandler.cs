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

namespace RoyalStore.Application.Tenants.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    IIdentityService identityService,
    IEmailService emailService,
    IPasswordResetRepository passwordResetRepository,
    ISecurityAuditService securityAuditService,
    IUnitOfWork unitOfWork,
    IOptions<PasswordResetOptions> options)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly PasswordResetOptions _options = options.Value;

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailAsync(command.Email);

        if (user is null)
            return Result.Success();

        var since = DateTimeOffset.UtcNow.AddMinutes(-_options.WindowMinutes);
        var recentCount = await passwordResetRepository.CountRecentAsync(
            user.Id, user.TenantId, since, cancellationToken);

        if (recentCount >= _options.MaxAttemptsPerWindow)
        {
            await securityAuditService.RecordAsync(
                "reset.rate_limited",
                user.TenantId,
                $"Rate limit exceeded: {recentCount} attempts in the last {_options.WindowMinutes} minutes.",
                ObfuscateEmail(command.Email),
                cancellationToken);

            return Result.Failure(Error.RateLimit("auth.rate_limited",
                "Too many reset requests. Please try again later."));
        }

        var activeRequest = await passwordResetRepository.FindActiveByUserIdAsync(
            user.Id, user.TenantId, cancellationToken);

        if (activeRequest is not null)
        {
            activeRequest.Consume();
            await identityService.UpdateSecurityStampAsync(user);
            user = await identityService.FindByEmailAsync(command.Email);
            if (user is null)
                return Result.Success();
        }

        var token = await identityService.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var encodedEmail = Base64UrlEncode(Encoding.UTF8.GetBytes(command.Email));
        var resetUrl = $"{_options.ClientAppBaseUrl}/auth/reset-password?token={encodedToken}&email={encodedEmail}";

        try
        {
            await emailService.SendPasswordResetAsync(command.Email, resetUrl, cancellationToken);
        }
        catch (Exception ex)
        {
            await securityAuditService.RecordAsync(
                "reset.email_send_failed",
                user.TenantId,
                ex.Message,
                ObfuscateEmail(command.Email),
                cancellationToken);
        }

        var requestResult = PasswordResetRequest.Create(user.Id, user.TenantId);
        if (!requestResult.IsSuccess)
            return Result.Failure(requestResult.Error);

        await passwordResetRepository.AddAsync(requestResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string ObfuscateEmail(string email)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return Convert.ToHexString(hash)[..12];
    }

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
