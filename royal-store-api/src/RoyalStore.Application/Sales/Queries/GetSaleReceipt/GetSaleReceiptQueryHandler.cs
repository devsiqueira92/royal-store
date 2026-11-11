using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Sales.Queries.GetSaleReceipt;

public sealed class GetSaleReceiptQueryHandler
    : IRequestHandler<GetSaleReceiptQuery, Result<SaleReceiptResponse>>
{
    private readonly ISaleRepository _saleRepository;

    public GetSaleReceiptQueryHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Result<SaleReceiptResponse>> Handle(
        GetSaleReceiptQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _saleRepository.GetReceiptByIdAsync(request.SaleId, cancellationToken);
        if (result is null)
            return Error.NotFound("sale.not_found", "Sale not found.");

        return SaleReceiptResponse.From(
            result.Sale, result.StoreName, result.DiscountAmount, result.Items, result.Payments);
    }
}
