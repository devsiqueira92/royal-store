using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Labels.Queries.GetLabelPrintOptions;

public sealed class GetLabelPrintOptionsQueryHandler(ILabelRepository labelRepository)
    : IRequestHandler<GetLabelPrintOptionsQuery, Result<IReadOnlyList<OptionResponse>>>
{
    public async Task<Result<IReadOnlyList<OptionResponse>>> Handle(
        GetLabelPrintOptionsQuery query, CancellationToken cancellationToken)
    {
        var options = await labelRepository.GetPrintOptionsAsync(cancellationToken);
        return Result<IReadOnlyList<OptionResponse>>.Success(options);
    }
}
