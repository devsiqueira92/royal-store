namespace RoyalStore.Application.Products.DTOs;

/// <summary>The item (product or service) already holding a SKU in this store.</summary>
public sealed record SkuOwner(string Sku, string ProductName, bool IsActive);
