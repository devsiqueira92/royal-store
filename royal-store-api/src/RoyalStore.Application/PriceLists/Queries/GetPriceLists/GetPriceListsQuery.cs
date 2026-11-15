using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.PriceLists.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.PriceLists.Queries.GetPriceLists;

/// <param name="IsActive">Null returns every list; true or false narrows to that state.</param>
public record GetPriceListsQuery(bool? IsActive = null)
    : IRequest<Result<IReadOnlyList<PriceListResponse>>>;

public sealed class GetPriceListsQueryHandler
    : IRequestHandler<GetPriceListsQuery, Result<IReadOnlyList<PriceListResponse>>>
{
    private readonly IPriceListRepository _priceListRepository;

    public GetPriceListsQueryHandler(IPriceListRepository priceListRepository)
    {
        _priceListRepository = priceListRepository;
    }

    public async Task<Result<IReadOnlyList<PriceListResponse>>> Handle(
        GetPriceListsQuery query, CancellationToken cancellationToken)
    {
        // Already ordered default first, then by name.
        var lists = await _priceListRepository.GetAllAsync(query.IsActive, cancellationToken);
        var counts = await _priceListRepository.GetProductCountsAsync(cancellationToken);

        return Result<IReadOnlyList<PriceListResponse>>.Success(
            [.. lists.Select(l => PriceListResponse.From(l, counts.GetValueOrDefault(l.Id)))]);
    }
}
