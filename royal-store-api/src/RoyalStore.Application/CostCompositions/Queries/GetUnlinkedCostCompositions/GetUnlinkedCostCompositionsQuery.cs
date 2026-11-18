using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.CostCompositions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.CostCompositions.Queries.GetUnlinkedCostCompositions;

/// <summary>
/// The store's lists linked to no product (064 FR-025) — abandoned before the product was saved, or
/// replaced on a product. Nothing deletes them on its own, so this is how the manager finds them to
/// reuse (by naming one in a product save) or delete.
/// </summary>
public record GetUnlinkedCostCompositionsQuery()
    : IRequest<Result<IReadOnlyList<CostCompositionResponse>>>;

public sealed class GetUnlinkedCostCompositionsQueryHandler
    : IRequestHandler<GetUnlinkedCostCompositionsQuery, Result<IReadOnlyList<CostCompositionResponse>>>
{
    private readonly ICostCompositionRepository _costCompositionRepository;

    public GetUnlinkedCostCompositionsQueryHandler(ICostCompositionRepository costCompositionRepository)
    {
        _costCompositionRepository = costCompositionRepository;
    }

    public async Task<Result<IReadOnlyList<CostCompositionResponse>>> Handle(
        GetUnlinkedCostCompositionsQuery query, CancellationToken cancellationToken)
    {
        // Newest first; an empty store returns an empty list.
        var lists = await _costCompositionRepository.GetUnlinkedAsync(cancellationToken);

        return Result<IReadOnlyList<CostCompositionResponse>>.Success(
            [.. lists.Select(CostCompositionResponse.From)]);
    }
}
