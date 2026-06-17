using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Catalog.DTOs;

public record CatalogCategoryDto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTimeOffset UpdatedAt)
{
    public static CatalogCategoryDto From(Category c) =>
        new(c.Id, c.Name, c.IsActive, c.UpdatedAt);
}
