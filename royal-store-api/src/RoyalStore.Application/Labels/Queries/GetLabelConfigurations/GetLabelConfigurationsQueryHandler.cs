using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabelConfigurations;

public sealed class GetLabelConfigurationsQueryHandler
    : IRequestHandler<GetLabelConfigurationsQuery, Result<IReadOnlyList<OptionResponse>>>
{
    private readonly ILabelRepository _labelRepository;

    public GetLabelConfigurationsQueryHandler(ILabelRepository labelRepository)
    {
        _labelRepository = labelRepository;
    }

    public async Task<Result<IReadOnlyList<OptionResponse>>> Handle(
        GetLabelConfigurationsQuery query, CancellationToken cancellationToken)
    {
        var options = await _labelRepository.GetConfigurationOptionsAsync(cancellationToken);
        return Result<IReadOnlyList<OptionResponse>>.Success(options);
    }
}
