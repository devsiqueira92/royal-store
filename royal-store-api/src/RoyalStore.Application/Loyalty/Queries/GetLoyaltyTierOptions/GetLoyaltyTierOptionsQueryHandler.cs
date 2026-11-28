using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Loyalty.Queries.GetLoyaltyTierOptions;

public sealed class GetLoyaltyTierOptionsQueryHandler(ILoyaltyTierRepository loyaltyTierRepository)
    : IRequestHandler<GetLoyaltyTierOptionsQuery, Result<IReadOnlyList<OptionResponse>>>
{
    public async Task<Result<IReadOnlyList<OptionResponse>>> Handle(
        GetLoyaltyTierOptionsQuery query, CancellationToken cancellationToken)
    {
        var options = await loyaltyTierRepository.GetActiveOptionsAsync(cancellationToken);
        return Result<IReadOnlyList<OptionResponse>>.Success(options);
    }
}
