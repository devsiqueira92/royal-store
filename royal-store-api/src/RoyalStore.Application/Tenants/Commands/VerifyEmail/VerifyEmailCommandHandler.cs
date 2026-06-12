using System.Text;
using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Tenants.Commands.VerifyEmail;

public sealed class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VerifyEmailCommandHandler(
        IIdentityService identityService,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByIdAsync(command.UserId);
        if (user is null)
            return Result.Failure(Error.Validation("verification.invalid_token",
                "The verification link is invalid or has expired. Please request a new verification email."));

        if (user.EmailConfirmed)
            return Result.Failure(Error.Validation("verification.already_verified",
                "This account has already been verified. You can log in."));

        string decodedToken;
        try
        {
            decodedToken = Base64UrlDecode(command.Token);
        }
        catch
        {
            return Result.Failure(Error.Validation("verification.invalid_token",
                "The verification link is invalid or has expired. Please request a new verification email."));
        }

        var confirmResult = await _identityService.ConfirmEmailAsync(user, decodedToken);
        if (!confirmResult.IsSuccess)
            return confirmResult;

        var tenant = await _tenantRepository.FindByIdAsync(user.TenantId, cancellationToken);
        if (tenant is null)
            return Result.Failure(Error.Unexpected("verification.tenant_not_found",
                "Tenant not found for the verified user."));

        tenant.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string Base64UrlDecode(string encoded)
    {
        var base64 = encoded.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
    }
}
