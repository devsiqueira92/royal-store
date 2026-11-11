using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetSaleDetails;

public sealed class GetSaleDetailsQueryHandler
    : IRequestHandler<GetSaleDetailsQuery, Result<SaleDetailsResponse>>
{
    private readonly ISaleRepository _saleRepository;

    public GetSaleDetailsQueryHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Result<SaleDetailsResponse>> Handle(
        GetSaleDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _saleRepository.GetDetailsByIdAsync(request.SaleId, cancellationToken);
        if (result is null)
            return Error.NotFound("sale.not_found", "Sale not found.");

        return SaleDetailsResponse.From(result.Sale, result.SellerName);
    }
}
