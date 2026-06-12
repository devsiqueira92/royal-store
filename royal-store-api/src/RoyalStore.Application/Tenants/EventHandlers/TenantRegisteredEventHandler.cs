using System.Text;
using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Email;
using RoyalStore.Application.Tenants.Events;

namespace RoyalStore.Application.Tenants.EventHandlers;

public sealed class TenantRegisteredEventHandler(
    IIdentityService identityService,
    IEmailService emailService)
    : INotificationHandler<TenantRegisteredEvent>
{
    public async Task Handle(TenantRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var user = await identityService.FindByIdAsync(notification.UserId);
        if (user is null) return;

        var token = await identityService.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        await emailService.SendEmailVerificationAsync(notification.Email, notification.UserId, encodedToken, cancellationToken);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
