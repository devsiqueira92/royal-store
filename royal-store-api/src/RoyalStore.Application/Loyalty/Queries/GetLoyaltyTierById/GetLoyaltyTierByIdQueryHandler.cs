using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Loyalty.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Queries.GetLoyaltyTierById;

public sealed class GetLoyaltyTierByIdQueryHandler
    : IRequestHandler<GetLoyaltyTierByIdQuery, Result<LoyaltyTierResponse>>
{
    private readonly ILoyaltyTierRepository _loyaltyTierRepository;

    public GetLoyaltyTierByIdQueryHandler(ILoyaltyTierRepository loyaltyTierRepository)
    {
        _loyaltyTierRepository = loyaltyTierRepository;
    }

    public async Task<Result<LoyaltyTierResponse>> Handle(
        GetLoyaltyTierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var tier = await _loyaltyTierRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tier is null)
            return Error.NotFound("loyaltytier.not_found", "Loyalty tier not found.");

        return LoyaltyTierResponse.From(tier);
    }
}
