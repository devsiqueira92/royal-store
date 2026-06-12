namespace RoyalStore.Application.Abstractions.Email;

public interface IEmailService
{
    Task SendEmailVerificationAsync(
        string toEmail,
        string userId,
        string encodedToken,
        CancellationToken cancellationToken = default);

    Task SendPasswordResetAsync(
        string toEmail,
        string resetUrl,
        CancellationToken cancellationToken = default);
}
