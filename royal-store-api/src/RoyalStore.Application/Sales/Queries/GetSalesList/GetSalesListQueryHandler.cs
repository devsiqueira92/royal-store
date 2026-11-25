using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Common;
using RoyalStore.Application.Common.Pagination;
using RoyalStore.Application.DTOs;
using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Queries.GetSalesList;

public sealed class GetSalesListQueryHandler(ISaleRepository saleRepository)
    : IRequestHandler<GetSalesListQuery, Result<CursorPageResponse<SaleListItemResponse>>>
{
    public async Task<Result<CursorPageResponse<SaleListItemResponse>>> Handle(
        GetSalesListQuery request,
        CancellationToken cancellationToken)
    {
        FulfillmentMode? fulfillmentMode = null;
        if (!string.IsNullOrWhiteSpace(request.FulfillmentMode))
        {
            var parsed = FulfillmentModeParser.Parse(request.FulfillmentMode);
            if (parsed.IsFailure)
                return Result<CursorPageResponse<SaleListItemResponse>>.Failure(parsed.Error);

            fulfillmentMode = parsed.Value;
        }

        var page = request.Page ?? new PageRequest();
        var after = CursorCodec.Decode<SalesListCursor>(page.Cursor);
        if (after.IsFailure)
            return Result<CursorPageResponse<SaleListItemResponse>>.Failure(after.Error);

        var sellerTerm = TextSearchNormalizer.Normalize(request.SellerName);
        var customerTerm = TextSearchNormalizer.Normalize(request.CustomerName);
        var filter = new SalesListFilter(
            sellerTerm.Length == 0 ? null : sellerTerm,
            customerTerm.Length == 0 ? null : customerTerm,
            request.SoldFrom,
            request.SoldTo,
            fulfillmentMode);

        // One extra row tells whether another page exists (067 FR-008).
        var rows = await saleRepository.GetPageAsync(filter, after.Value, page.Size + 1, cancellationToken);

        var response = CursorPage.Create(
            rows, page.Size, r => new SalesListCursor(r.SoldAt, r.Id), SaleListItemResponse.From);

        return Result<CursorPageResponse<SaleListItemResponse>>.Success(response);
    }
}
