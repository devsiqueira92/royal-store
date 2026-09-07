using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Queries.GetLoyaltyTiers;

public sealed class GetLoyaltyTiersQueryHandler
    : IRequestHandler<GetLoyaltyTiersQuery, Result<IReadOnlyList<LoyaltyTierResponse>>>
{
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;

    public GetLoyaltyTiersQueryHandler(ILoyaltyTierRepository loyaltyTierRepository)
    {
        _loyaltyTierRepository = loyaltyTierRepository;
    }

    public async Task<Result<IReadOnlyList<LoyaltyTierResponse>>> Handle(
        GetLoyaltyTiersQuery request,
        CancellationToken cancellationToken)
    {
        var tiers = await _loyaltyTierRepository.GetAllAsync(cancellationToken);
        IReadOnlyList<LoyaltyTierResponse> responses = tiers.Select(LoyaltyTierResponse.From).ToList();
        return Result<IReadOnlyList<LoyaltyTierResponse>>.Success(responses);
    }
}
