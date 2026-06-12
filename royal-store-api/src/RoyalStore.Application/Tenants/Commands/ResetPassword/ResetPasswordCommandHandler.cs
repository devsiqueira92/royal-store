using System.Security.Cryptography;
using System.Text;
using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Security;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Tenants.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IIdentityService identityService,
    IPasswordResetRepository passwordResetRepository,
    ISecurityAuditService securityAuditService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    private const string GenericInvalidTokenError = "auth.invalid_reset_token";
    private const string GenericInvalidTokenMessage =
        "The reset link is invalid or has expired. Please request a new one.";

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByEmailAsync(command.Email);

        if (user is null)
        {
            await securityAuditService.RecordAsync(
                "reset.cross_tenant",
                Guid.Empty,
                "Reset attempt for unrecognised email.",
                ObfuscateEmail(command.Email),
                cancellationToken);

            return Result.Failure(Error.Validation(GenericInvalidTokenError, GenericInvalidTokenMessage));
        }

        string decodedToken;
        try
        {
            decodedToken = Base64UrlDecode(command.Token);
        }
        catch
        {
            await securityAuditService.RecordAsync(
                "reset.token_invalid",
                user.TenantId,
                "Token could not be Base64Url decoded.",
                ObfuscateEmail(command.Email),
                cancellationToken);

            return Result.Failure(Error.Validation(GenericInvalidTokenError, GenericInvalidTokenMessage));
        }

        var resetResult = await identityService.ResetPasswordAsync(user, decodedToken, command.NewPassword);
        if (!resetResult.IsSuccess)
        {
            await securityAuditService.RecordAsync(
                "reset.token_invalid",
                user.TenantId,
                resetResult.Error.Message,
                ObfuscateEmail(command.Email),
                cancellationToken);

            return Result.Failure(Error.Validation(GenericInvalidTokenError, GenericInvalidTokenMessage));
        }

        var activeRequest = await passwordResetRepository.FindActiveByUserIdAsync(
            user.Id, user.TenantId, cancellationToken);

        activeRequest?.Consume();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string ObfuscateEmail(string email)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant()));
        return Convert.ToHexString(hash)[..12];
    }

    private static string Base64UrlDecode(string encoded)
    {
        var base64 = encoded.Replace('-', '+').Replace('_', '/');
        var paddedBase64 = (base64.Length % 4) switch
        {
            2 => base64 + "==",
            3 => base64 + "=",
            _ => base64
        };
        return Encoding.UTF8.GetString(Convert.FromBase64String(paddedBase64));
    }
}
