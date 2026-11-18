using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.CostCompositions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.CostCompositions.Queries.GetCostCompositions;

/// <summary>
/// Every cost composition list of the store, linked to a product or not (064 FR-028), one summary row
/// each. The lines are not returned: open a list by its identifier for those.
/// </summary>
public record GetCostCompositionsQuery()
    : IRequest<Result<IReadOnlyList<CostCompositionSummaryResponse>>>;

public sealed class GetCostCompositionsQueryHandler
    : IRequestHandler<GetCostCompositionsQuery, Result<IReadOnlyList<CostCompositionSummaryResponse>>>
{
    private readonly ICostCompositionRepository _costCompositionRepository;

    public GetCostCompositionsQueryHandler(ICostCompositionRepository costCompositionRepository)
    {
        _costCompositionRepository = costCompositionRepository;
    }

    public async Task<Result<IReadOnlyList<CostCompositionSummaryResponse>>> Handle(
        GetCostCompositionsQuery query, CancellationToken cancellationToken)
    {
        // Ordered by name; the tenant filter limits this to the caller's store.
        var summaries = await _costCompositionRepository.GetSummariesAsync(cancellationToken);

        return Result<IReadOnlyList<CostCompositionSummaryResponse>>.Success(
            [.. summaries.Select(CostCompositionSummaryResponse.From)]);
    }
}
