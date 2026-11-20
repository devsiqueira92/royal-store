using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Labels.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabels;

public sealed class GetLabelsQueryHandler
    : IRequestHandler<GetLabelsQuery, Result<IReadOnlyList<LabelResponse>>>
{
    private readonly ILabelRepository _labelRepository;

    public GetLabelsQueryHandler(ILabelRepository labelRepository)
    {
        _labelRepository = labelRepository;
    }

    public async Task<Result<IReadOnlyList<LabelResponse>>> Handle(
        GetLabelsQuery query, CancellationToken cancellationToken)
    {
        var labels = await _labelRepository.GetAllAsync(query.IsActive, cancellationToken);
        return Result<IReadOnlyList<LabelResponse>>.Success(labels.Select(LabelResponse.From).ToList());
    }
}
