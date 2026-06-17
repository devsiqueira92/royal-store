namespace RoyalStore.Application.Catalog.DTOs;

public record CatalogSyncResponse(
    IReadOnlyList<CatalogProductDto> Products,
    IReadOnlyList<CatalogCategoryDto> Categories);
