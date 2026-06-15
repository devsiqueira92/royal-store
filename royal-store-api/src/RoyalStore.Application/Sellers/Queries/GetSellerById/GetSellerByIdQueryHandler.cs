using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sellers.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sellers.Queries.GetSellerById;

public sealed class GetSellerByIdQueryHandler
    : IRequestHandler<GetSellerByIdQuery, Result<SellerResponse>>
{
    private readonly ISellerRepository _sellerRepository;

    public GetSellerByIdQueryHandler(ISellerRepository sellerRepository)
    {
        _sellerRepository = sellerRepository;
    }

    public async Task<Result<SellerResponse>> Handle(
        GetSellerByIdQuery request,
        CancellationToken cancellationToken)
    {
        var seller = await _sellerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (seller is null)
            return Error.NotFound("seller.not_found", "Seller not found.");

        return SellerResponse.From(seller);
    }
}
