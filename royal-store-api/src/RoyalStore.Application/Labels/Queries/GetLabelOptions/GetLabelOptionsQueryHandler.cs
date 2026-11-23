using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabelOptions;

public sealed class GetLabelOptionsQueryHandler
    : IRequestHandler<GetLabelOptionsQuery, Result<IReadOnlyList<OptionResponse>>>
{
    private readonly ILabelRepository _labelRepository;

    public GetLabelOptionsQueryHandler(ILabelRepository labelRepository)
    {
        _labelRepository = labelRepository;
    }

    public async Task<Result<IReadOnlyList<OptionResponse>>> Handle(
        GetLabelOptionsQuery query, CancellationToken cancellationToken)
    {
        var options = await _labelRepository.GetActiveOptionsAsync(cancellationToken);
        return Result<IReadOnlyList<OptionResponse>>.Success(options);
    }
}
