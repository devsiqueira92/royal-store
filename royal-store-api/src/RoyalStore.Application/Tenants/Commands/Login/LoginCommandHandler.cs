using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Tenants.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IIdentityService identityService, ITokenService tokenService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
    }

    public async Task<Result<TokenResponse>> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await _identityService.FindByEmailAsync(command.Email);
        if (user is null)
            return Error.Unauthorized("auth.invalid_credentials", "Invalid email or password.");

        var passwordValid = await _identityService.CheckPasswordAsync(user, command.Password);
        if (!passwordValid)
            return Error.Unauthorized("auth.invalid_credentials", "Invalid email or password.");

        if (!user.EmailConfirmed)
            return Error.Unauthorized("auth.email_not_verified",
                "Please verify your email before logging in.");

        var roles = await _identityService.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        return _tokenService.GenerateToken(user, role);
    }
}
