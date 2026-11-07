using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.Application.Tenants.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IProfileRepository _profileRepository;
    private readonly IPermissionGrantRepository _permissionGrantRepository;

    public LoginCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        IProfileRepository profileRepository,
        IPermissionGrantRepository permissionGrantRepository)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _profileRepository = profileRepository;
        _permissionGrantRepository = permissionGrantRepository;
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

        var profile = await _profileRepository.GetByUserIdAsync(user.Id, cancellationToken);

        var onboardingCompleted = role != Role.Seller || profile?.IsComplete is true;
        var isProfileComplete = profile?.IsComplete is true;

        var grants = await _permissionGrantRepository.GetPermissionsByUserIdAsync(user.Id, cancellationToken);
        var permissions = EffectivePermissions.For(role, grants)
            .Select(PermissionCatalog.NameOf)
            .ToArray();

        return _tokenService.GenerateToken(user, role, onboardingCompleted, isProfileComplete, permissions);
    }
}
