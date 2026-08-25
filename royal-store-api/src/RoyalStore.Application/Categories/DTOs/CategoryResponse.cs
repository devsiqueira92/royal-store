using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Categories.DTOs;

public record CategoryResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    string? Color,
    bool IsActive,
    int ProductCount)
{
    public static CategoryResponse From(Category category, int productCount = 0) =>
        new(category.Id, category.TenantId, category.Name, category.Description, category.Color, category.IsActive, productCount);
}
