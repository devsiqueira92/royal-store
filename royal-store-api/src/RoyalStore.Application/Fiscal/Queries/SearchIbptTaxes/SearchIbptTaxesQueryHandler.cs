using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Common;
using RoyalStore.Application.Fiscal.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Fiscal.Queries.SearchIbptTaxes;

public sealed class SearchIbptTaxesQueryHandler
    : IRequestHandler<SearchIbptTaxesQuery, Result<IReadOnlyList<IbptTaxResponse>>>
{
    private const int MaxResults = 30;

    private readonly IIbptTaxRepository _repository;

    public SearchIbptTaxesQueryHandler(IIbptTaxRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<IbptTaxResponse>>> Handle(
        SearchIbptTaxesQuery request,
        CancellationToken cancellationToken)
    {
        var term = TextSearchNormalizer.Normalize(request.Q);

        var entries = await _repository.SearchAsync(term, MaxResults, cancellationToken);

        IReadOnlyList<IbptTaxResponse> response = entries.Select(IbptTaxResponse.From).ToList();

        return Result<IReadOnlyList<IbptTaxResponse>>.Success(response);
    }
}
