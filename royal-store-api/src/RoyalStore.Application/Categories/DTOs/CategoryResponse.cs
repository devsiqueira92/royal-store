using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.Categories.DTOs;

public record CategoryResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string? Description,
    bool IsActive)
{
    public static CategoryResponse From(Category category) =>
        new(category.Id, category.TenantId, category.Name, category.Description, category.IsActive);
}
