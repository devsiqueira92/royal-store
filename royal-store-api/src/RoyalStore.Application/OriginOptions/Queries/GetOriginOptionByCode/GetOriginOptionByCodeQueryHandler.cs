using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.OriginOptions.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.OriginOptions.Queries.GetOriginOptionByCode;

public sealed class GetOriginOptionByCodeQueryHandler
    : IRequestHandler<GetOriginOptionByCodeQuery, Result<OriginOptionResponse>>
{
    private readonly IOriginOptionRepository _repository;

    public GetOriginOptionByCodeQueryHandler(IOriginOptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<OriginOptionResponse>> Handle(
        GetOriginOptionByCodeQuery request,
        CancellationToken cancellationToken)
    {
        var option = await _repository.GetByCodeAsync(request.Code, cancellationToken);
        if (option is null)
            return Error.NotFound("origin_option.not_found", "Origin option not found.");

        return OriginOptionResponse.From(option);
    }
}
