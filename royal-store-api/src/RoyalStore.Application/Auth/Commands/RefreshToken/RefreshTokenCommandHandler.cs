using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Const;
using RoyalStore.Domain.Permissions;

namespace RoyalStore.Application.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenService _tokenService;
    private readonly IProfileRepository _profileRepository;
    private readonly IPermissionGrantRepository _permissionGrantRepository;
    private readonly ICurrentUserService _currentUserService;

    public RefreshTokenCommandHandler(
        IIdentityService identityService,
        ITokenService tokenService,
        IProfileRepository profileRepository,
        IPermissionGrantRepository permissionGrantRepository,
        ICurrentUserService currentUserService)
    {
        _identityService = identityService;
        _tokenService = tokenService;
        _profileRepository = profileRepository;
        _permissionGrantRepository = permissionGrantRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<TokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Error.Unauthorized("auth.unauthenticated", "Not authenticated.");

        var user = await _identityService.FindByIdAsync(userId);
        if (user is null)
            return Error.Unauthorized("auth.unauthenticated", "Not authenticated.");

        var roles = await _identityService.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

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
