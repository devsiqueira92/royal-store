using Microsoft.Extensions.Logging;
using RoyalStore.Application.Abstractions.Email;

namespace RoyalStore.Infrastructure.Email;

public sealed class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;

    public ConsoleEmailService(ILogger<ConsoleEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(
        string toEmail,
        string userId,
        string encodedToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Verification link for {Email}: /auth/verify-email?userId={UserId}&token={Token}",
            toEmail, userId, encodedToken);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EMAIL STUB] Password reset link for {Email}: {ResetUrl}",
            toEmail, resetUrl);

        return Task.CompletedTask;
    }
}
