using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.OriginOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.OriginOptions.Queries.GetOriginOptions;

public sealed class GetOriginOptionsQueryHandler
    : IRequestHandler<GetOriginOptionsQuery, Result<IReadOnlyList<OriginOptionResponse>>>
{
    private readonly IOriginOptionRepository _repository;

    public GetOriginOptionsQueryHandler(IOriginOptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<OriginOptionResponse>>> Handle(
        GetOriginOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var options = await _repository.GetAllAsync(cancellationToken);

        IReadOnlyList<OriginOptionResponse> response = options
            .Select(OriginOptionResponse.From)
            .ToList();

        return Result<IReadOnlyList<OriginOptionResponse>>.Success(response);
    }
}
