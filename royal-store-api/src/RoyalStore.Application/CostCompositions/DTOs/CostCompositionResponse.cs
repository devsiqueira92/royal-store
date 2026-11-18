using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.CostCompositions.DTOs;

/// <summary>A cost composition list and its lines (064).</summary>
/// <param name="Name">What the manager calls the list.</param>
/// <param name="ProductId">The product the list is linked to; null until a product save names it.</param>
/// <param name="CreatedAt">When the list was created.</param>
/// <param name="TotalValue">The sum of the lines' amounts, as stored on the list.</param>
/// <param name="Items">The lines, in the order they were saved.</param>
public record CostCompositionResponse(
    Guid Id,
    string Name,
    Guid? ProductId,
    DateTimeOffset CreatedAt,
    decimal TotalValue,
    IReadOnlyList<ProductCostItemResponse> Items)
{
    public static CostCompositionResponse From(CostCompositionList list) =>
        new(list.Id, list.Name, list.ProductId, list.CreatedAt, list.TotalValue,
            // Ordered in memory, not by a filtered Include (063 research D4).
            list.Items
                .OrderBy(i => i.SortOrder)
                .Select(ProductCostItemResponse.From)
                .ToList());
}
