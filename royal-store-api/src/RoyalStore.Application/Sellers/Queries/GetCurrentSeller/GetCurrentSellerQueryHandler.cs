using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Services;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Queries.GetCurrentSeller;

public sealed class GetCurrentSellerQueryHandler
    : IRequestHandler<GetCurrentSellerQuery, Result<CurrentSellerResponse>>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IProfileRepository _profileRepository;
    private readonly IIdentityService _identityService;

    public GetCurrentSellerQueryHandler(
        ICurrentUserService currentUserService,
        IProfileRepository profileRepository,
        IIdentityService identityService)
    {
        _currentUserService = currentUserService;
        _profileRepository = profileRepository;
        _identityService = identityService;
    }

    public async Task<Result<CurrentSellerResponse>> Handle(
        GetCurrentSellerQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.CurrentUserId;
        if (string.IsNullOrWhiteSpace(userId))
            return Error.Unauthorized("auth.not_authenticated", "No authenticated user.");

        var profile = await _profileRepository.GetByUserIdAsync(userId, cancellationToken);

        return CurrentSellerResponse.From(profile);
    }
}
