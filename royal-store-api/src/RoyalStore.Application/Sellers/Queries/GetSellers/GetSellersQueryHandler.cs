using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Queries.GetSellers;

public sealed class GetSellersQueryHandler
    : IRequestHandler<GetSellersQuery, Result<IReadOnlyList<SellerResponse>>>
{
    private readonly ISellerRepository _sellerRepository;

    public GetSellersQueryHandler(ISellerRepository sellerRepository)
    {
        _sellerRepository = sellerRepository;
    }

    public async Task<Result<IReadOnlyList<SellerResponse>>> Handle(
        GetSellersQuery request,
        CancellationToken cancellationToken)
    {
        var sellers = await _sellerRepository.GetAllAsync(request.IsActive, cancellationToken);
        IReadOnlyList<SellerResponse> response = sellers.Select(SellerResponse.From).ToList();
        return Result<IReadOnlyList<SellerResponse>>.Success(response);
    }
}
