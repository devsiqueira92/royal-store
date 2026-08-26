using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Products.DTOs;

public record ProductTagResponse(
    Guid CategoryId,
    string CategoryName,
    string? CategoryColor)
{
    public static ProductTagResponse From(ProductTag t) =>
        new(t.CategoryId, t.Category.Name, t.Category.Color);
}
