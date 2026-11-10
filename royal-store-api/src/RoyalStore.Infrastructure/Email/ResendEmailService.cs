using Microsoft.Extensions.Options;
using Resend;
using RoyalStore.Application.Abstractions.Email;
using RoyalStore.Application.Options;

namespace RoyalStore.Infrastructure.Email;

public sealed class ResendEmailService(
    IResend resend,
    IOptions<ResendOptions> resendOptions,
    IOptions<PasswordResetOptions> passwordResetOptions)
    : IEmailService
{
    public async Task SendEmailVerificationAsync(
        string toEmail,
        string userId,
        string encodedToken,
        CancellationToken cancellationToken = default)
    {
        var verificationUrl =
            $"{passwordResetOptions.Value.ClientAppBaseUrl}/auth/verify-email?userId={userId}&token={encodedToken}";

        var message = BuildMessage(
            toEmail,
            "Confirm your account",
            $"<p>Click the link below to confirm your account:</p><p><a href=\"{verificationUrl}\">{verificationUrl}</a></p>");

        await resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendPasswordResetAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        var message = BuildMessage(
            toEmail,
            "Reset your password",
            $"<p>Click the link below to reset your password:</p><p><a href=\"{resetUrl}\">{resetUrl}</a></p>");

        await resend.EmailSendAsync(message, cancellationToken);
    }

    private EmailMessage BuildMessage(string toEmail, string subject, string htmlBody) =>
        new()
        {
            From = $"{resendOptions.Value.FromName} <{resendOptions.Value.FromEmail}>",
            To = toEmail,
            Subject = subject,
            HtmlBody = htmlBody,
        };
}
