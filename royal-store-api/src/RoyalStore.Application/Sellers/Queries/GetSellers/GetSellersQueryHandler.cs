using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Profiles;

namespace RoyalStore.Application.Sellers.Queries.GetSellers;

public sealed class GetSellersQueryHandler
    : IRequestHandler<GetSellersQuery, Result<IReadOnlyList<SellerResponse>>>
{
    private readonly ISellerRepository _sellerRepository;
    private readonly IProfileRepository _profileRepository;

    public GetSellersQueryHandler(ISellerRepository sellerRepository, IProfileRepository profileRepository)
    {
        _sellerRepository = sellerRepository;
        _profileRepository = profileRepository;
    }

    public async Task<Result<IReadOnlyList<SellerResponse>>> Handle(
        GetSellersQuery request,
        CancellationToken cancellationToken)
    {
        var sellers = await _sellerRepository.GetAllAsync(request.IsActive, cancellationToken);
        var userIds = sellers.Where(s => s.UserId is not null).Select(s => s.UserId!);
        var profilesByUserId = await _profileRepository.GetByUserIdsAsync(userIds, cancellationToken);

        IReadOnlyList<SellerResponse> response = sellers
            .Select(s => SellerResponse.From(
                s, s.UserId is not null && profilesByUserId.TryGetValue(s.UserId, out var p) ? p : null))
            .ToList();
        return Result<IReadOnlyList<SellerResponse>>.Success(response);
    }
}
