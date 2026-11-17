using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

/// <summary>One line of a product's cost breakdown (063). Array position is the stored order.</summary>
public record ProductCostItemResponse(Guid Id, string Description, decimal Value)
{
    public static ProductCostItemResponse From(ProductCostItem item) =>
        new(item.Id, item.Description, item.Value);
}
