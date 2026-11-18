using RoyalStore.Application.Abstractions.Data;

namespace RoyalStore.Application.CostCompositions.DTOs;

/// <summary>A cost composition list as one row of the store's listing (064).</summary>
/// <param name="Id">Identifies the list, so a row can be opened, changed or named in a product save.</param>
/// <param name="ProductId">The product the list is linked to; null when it is linked to none.</param>
/// <param name="TotalValue">The sum of the lines' amounts, as stored on the list.</param>
/// <param name="NumberItems">How many lines the list has.</param>
public record CostCompositionSummaryResponse(
    Guid Id,
    Guid? ProductId,
    string Name,
    decimal TotalValue,
    int NumberItems)
{
    public static CostCompositionSummaryResponse From(CostCompositionSummary summary) =>
        new(summary.Id, summary.ProductId, summary.Name, summary.TotalValue, summary.NumberItems);
}
