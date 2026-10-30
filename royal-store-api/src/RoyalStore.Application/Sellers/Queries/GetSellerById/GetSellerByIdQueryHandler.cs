using MediatR;
using RoyalStore.Application.Abstractions.Authentication;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Permissions;
using RoyalStore.Domain.Profiles;

namespace RoyalStore.Application.Sellers.Queries.GetSellerById;

public sealed class GetSellerByIdQueryHandler
    : IRequestHandler<GetSellerByIdQuery, Result<SellerResponse>>
{
    private readonly ISellerRepository _sellerRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IIdentityService _identityService;
    private readonly IPermissionGrantRepository _permissionGrantRepository;

    public GetSellerByIdQueryHandler(
        ISellerRepository sellerRepository,
        IProfileRepository profileRepository,
        IIdentityService identityService,
        IPermissionGrantRepository permissionGrantRepository)
    {
        _sellerRepository = sellerRepository;
        _profileRepository = profileRepository;
        _identityService = identityService;
        _permissionGrantRepository = permissionGrantRepository;
    }

    public async Task<Result<SellerResponse>> Handle(
        GetSellerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (seller is null)
            return Error.NotFound("seller.not_found", "Seller not found.");

        Profile? profile = null;
        string? role = null;
        IReadOnlyList<string> permissions = [];

        if (!string.IsNullOrWhiteSpace(seller.UserId))
        {
            profile = await _profileRepository.GetByUserIdAsync(seller.UserId, cancellationToken);

            var user = await _identityService.FindByIdAsync(seller.UserId);
            if (user is not null)
            {
                var roles = await _identityService.GetRolesAsync(user);
                role = roles.FirstOrDefault();
            }

            var grants = await _permissionGrantRepository.GetPermissionsByUserIdAsync(
                seller.UserId, cancellationToken);
            permissions = grants
                .Select(PermissionCatalog.NameOf)
                .OrderBy(x => x, StringComparer.Ordinal)
                .ToArray();
        }

        return SellerResponse.From(seller, profile) with { Role = role, Permissions = permissions };
    }
}
