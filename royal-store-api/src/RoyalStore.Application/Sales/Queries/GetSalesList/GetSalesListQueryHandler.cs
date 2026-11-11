using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Common;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Queries.GetSalesList;

public sealed class GetSalesListQueryHandler
    : IRequestHandler<GetSalesListQuery, Result<IReadOnlyList<SaleListItemResponse>>>
{
    private readonly ISaleRepository _saleRepository;

    public GetSalesListQueryHandler(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Result<IReadOnlyList<SaleListItemResponse>>> Handle(
        GetSalesListQuery request,
        CancellationToken cancellationToken)
    {
        FulfillmentMode? fulfillmentMode = null;
        if (!string.IsNullOrWhiteSpace(request.FulfillmentMode))
        {
            var parsed = FulfillmentModeParser.Parse(request.FulfillmentMode);
            if (parsed.IsFailure)
                return Result<IReadOnlyList<SaleListItemResponse>>.Failure(parsed.Error);

            fulfillmentMode = parsed.Value;
        }

        var sellerTerm = TextSearchNormalizer.Normalize(request.SellerName);
        var customerTerm = TextSearchNormalizer.Normalize(request.CustomerName);

        var sales = await _saleRepository.GetAllAsync(
            sellerTerm.Length == 0 ? null : sellerTerm,
            customerTerm.Length == 0 ? null : customerTerm,
            request.SoldFrom,
            request.SoldTo,
            fulfillmentMode,
            cancellationToken);

        IReadOnlyList<SaleListItemResponse> response = sales
            .Select(SaleListItemResponse.From)
            .ToList();

        return Result<IReadOnlyList<SaleListItemResponse>>.Success(response);
    }
}
