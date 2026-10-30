using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Profiles;

namespace RoyalStore.Application.Sellers.Commands.UpdateMyProfile;

public sealed class UpdateMyProfileCommandHandler
    : IRequestHandler<UpdateMyProfileCommand, Result<CurrentSellerResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IProfileRepository _profileRepository;
    private readonly IIdentityService _identityService;
    private readonly ITenantContext _tenantContext;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMyProfileCommandHandler(
        ICurrentUserService currentUserService,
        IProfileRepository profileRepository,
        IIdentityService identityService,
        ITenantContext tenantContext,
        IUnitOfWork unitOfWork)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _identityService = identityService;
        _tenantContext = tenantContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CurrentSellerResponse>> Handle(
        UpdateMyProfileCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Error.Unauthorized("auth.not_authenticated", "No authenticated user.");

        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);

        if (profile is not null)
        {
            var updateResult = profile.UpdateProfile(command.FullName, command.Phone);
            if (!updateResult.IsSuccess)
                return updateResult.Error;

            await _profileRepository.UpdateAsync(profile, cancellationToken);
        }
        else
        {
            var createResult = Profile.CreateOnboarded(
                _tenantContext.TenantId, command.FullName, command.Phone, userId);
            if (!createResult.IsSuccess)
                return createResult.Error;

            profile = createResult.Value;
            await _profileRepository.AddAsync(profile, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _identityService.FindByIdAsync(userId);

        return CurrentSellerResponse.From(profile);
    }
}
