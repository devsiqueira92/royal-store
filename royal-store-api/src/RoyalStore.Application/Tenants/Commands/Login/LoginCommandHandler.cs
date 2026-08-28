using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Const;

namespace RoyalStore.Application.Tenants.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly ISellerRepository _sellerRepository;

    public LoginCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        ISellerRepository sellerRepository)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _sellerRepository = sellerRepository;
    }

    public async Task<Result<TokenResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByEmailAsync(command.Identifier)
                   ?? await _identityService.FindByNameAsync(command.Identifier);
        if (user is null)
            return Error.Unauthorized("auth.invalid_credentials", "Invalid credentials.");

        var passwordValid = await _identityService.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
            return Error.Unauthorized("auth.invalid_credentials", "Invalid credentials.");

        var roles = await _identityService.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        // Seller accounts are provisioned without an email and authenticate by
        // username; the confirmed-email gate does not apply to them.
        if (role != Role.Seller && !user.EmailConfirmed)
            return Error.Unauthorized("auth.email_not_verified",
                "Please verify your email before logging in.");

        var onboardingCompleted = role != Role.Seller
            || (await _sellerRepository.GetByUserIdAsync(user.Id, cancellationToken))?.OnboardingCompletedAt is not null;

        return _tokenService.GenerateToken(user, role, onboardingCompleted);
    }
}
