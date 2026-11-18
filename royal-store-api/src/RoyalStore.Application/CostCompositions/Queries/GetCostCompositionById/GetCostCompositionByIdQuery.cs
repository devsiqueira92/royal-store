using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.CostCompositions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.CostCompositions.Queries.GetCostCompositionById;

public record GetCostCompositionByIdQuery(Guid Id) : IRequest<Result<CostCompositionResponse>>;

public sealed class GetCostCompositionByIdQueryHandler
    : IRequestHandler<GetCostCompositionByIdQuery, Result<CostCompositionResponse>>
{
    private readonly ICostCompositionRepository _costCompositionRepository;

    public GetCostCompositionByIdQueryHandler(ICostCompositionRepository costCompositionRepository)
    {
        _costCompositionRepository = costCompositionRepository;
    }

    public async Task<Result<CostCompositionResponse>> Handle(
        GetCostCompositionByIdQuery query, CancellationToken cancellationToken)
    {
        // A list of another store comes back as null: the tenant filter hides it (064 FR-002).
        var list = await _costCompositionRepository.GetByIdAsync(query.Id, cancellationToken);
        if (list is null)
            return Error.NotFound("costcomposition.not_found", "Cost composition was not found.");

        return CostCompositionResponse.From(list);
    }
}
