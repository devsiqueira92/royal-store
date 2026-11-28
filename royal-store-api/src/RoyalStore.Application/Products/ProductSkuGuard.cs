using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Products.DTOs;
using RoyalStore.Domain.Common;

namespace RoyalStore.Application.Products;

/// <summary>
/// Checks, before saving, the SKUs the database keeps unique per store — the item's own SKU and its
/// variants' — so a clash is told to the user with what holds the SKU, instead of surfacing as a
/// unique-index violation (a generic error). The unique indexes cover inactive items too, so an
/// inactive owner is reported as well, with what to do about it.
/// </summary>
public static class ProductSkuGuard
{
    public const string SkuTakenCode = "product.sku_taken";
    public const string SkuTakenByInactiveCode = "product.sku_taken_by_inactive";
    public const string VariantSkuTakenCode = "product.variant_sku_taken";
    public const string VariantSkuRepeatedCode = "product.variant_sku_repeated";

    /// <param name="sku">The item's SKU, as submitted.</param>
    /// <param name="variantSkus">Every SKU the item's variants will hold once saved (nulls ignored).</param>
    /// <param name="productId">The item being edited, whose own SKUs never clash; null when creating.</param>
    public static async Task<Result> EnsureAvailableAsync(
        IProductRepository productRepository,
        string sku,
        IEnumerable<string?> variantSkus,
        Guid? productId,
        CancellationToken cancellationToken)
    {
        var itemSku = sku.Trim();
        var owner = await productRepository.FindSkuOwnerAsync(itemSku, productId, cancellationToken);
        if (owner is not null)
            return Result.Failure(owner.IsActive
                ? Error.Conflict(SkuTakenCode,
                    $"SKU '{itemSku}' is already used by the active item '{owner.ProductName}'.")
                : Error.Conflict(SkuTakenByInactiveCode,
                    $"SKU '{itemSku}' belongs to the inactive item '{owner.ProductName}'. Reactivate it or use another SKU."));

        var skus = variantSkus
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s!.Trim())
            .ToList();

        var repeated = skus.GroupBy(s => s).FirstOrDefault(g => g.Count() > 1);
        if (repeated is not null)
            return Result.Failure(Error.Conflict(VariantSkuRepeatedCode,
                $"SKU '{repeated.Key}' is used by more than one variant of this item."));

        if (skus.Count == 0)
            return Result.Success();

        var variantOwner = (await productRepository.FindVariantSkuOwnersAsync(skus, productId, cancellationToken))
            .FirstOrDefault();

        return variantOwner is null
            ? Result.Success()
            : Result.Failure(Error.Conflict(VariantSkuTakenCode,
                $"Variant SKU '{variantOwner.Sku}' is already used by the {(variantOwner.IsActive ? "active" : "inactive")} item '{variantOwner.ProductName}'."));
    }
}
