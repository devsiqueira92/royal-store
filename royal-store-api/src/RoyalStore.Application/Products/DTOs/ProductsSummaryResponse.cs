using RoyalStore.Application.Abstractions.Data;

namespace RoyalStore.Application.Products.DTOs;

public record ProductsSummaryResponse(decimal StockValue, decimal TotalQuantity, int Result)
{
    public static ProductsSummaryResponse From(ProductsSummary summary) =>
        new(summary.StockValue, summary.TotalQuantity, summary.Result);
}
