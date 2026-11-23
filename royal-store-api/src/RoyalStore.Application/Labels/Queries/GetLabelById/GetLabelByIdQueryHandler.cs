using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabelById;

public sealed class GetLabelByIdQueryHandler : IRequestHandler<GetLabelByIdQuery, Result<LabelResponse>>
{
    private readonly ILabelRepository _labelRepository;

    public GetLabelByIdQueryHandler(ILabelRepository labelRepository)
    {
        _labelRepository = labelRepository;
    }

    public async Task<Result<LabelResponse>> Handle(GetLabelByIdQuery query, CancellationToken cancellationToken)
    {
        // Another store's label is filtered out by the tenant query filter: same answer as a
        // missing one (065 FR-017). System configurations open for every store (066 FR-005).
        var label = await _labelRepository.GetByIdAsync(query.Id, cancellationToken);
        if (label is null)
            return LabelErrors.NotFound;

        return LabelResponse.From(label);
    }
}
