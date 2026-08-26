using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class Product : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public string UnitOfMeasure { get; private set; } = "UN";
    public string? Ncm { get; private set; }
    public string? Cest { get; private set; }
    public int Origin { get; private set; }

    public ICollection<ProductVariant> Variants { get; private set; } = [];
    public ICollection<ProductTag> Tags { get; private set; } = [];
    public ICollection<ProductPhoto> Photos { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Product() : base(Guid.NewGuid()) { }

    private Product(Guid id, Guid tenantId, Guid categoryId, string name, string? description,
        string unitOfMeasure, string? ncm, string? cest, int origin)
        : base(id)
    {
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        IsActive = true;
        UnitOfMeasure = unitOfMeasure;
        Ncm = ncm;
        Cest = cest;
        Origin = origin;
    }

    public static Result<Product> Create(
        Guid tenantId, Guid categoryId, string name, string? description,
        string unitOfMeasure, string? ncm, string? cest, int origin)
    {
        if (tenantId == Guid.Empty)
            return Error.Validation("product.tenant_required", "Tenant identifier is required.");

        if (categoryId == Guid.Empty)
            return Error.Validation("product.category_required", "Category is required.");

        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("product.name_required", "Product name is required.");

        name = name.Trim();

        if (name.Length > 120)
            return Error.Validation("product.name_too_long", "Product name must not exceed 120 characters.");

        if (description is not null && description.Length > 500)
            return Error.Validation("product.description_too_long", "Description must not exceed 500 characters.");

        if (origin is < 0 or > 8)
            return Error.Validation("product.origin_invalid", "Origin must be between 0 and 8.");

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            unitOfMeasure = "UN";

        return new Product(Guid.NewGuid(), tenantId, categoryId, name, description,
            unitOfMeasure, ncm, cest, origin);
    }

    public Result Update(
        Guid categoryId, string name, string? description,
        string unitOfMeasure, string? ncm, string? cest, int origin)
    {
        if (!IsActive)
            return Result.Failure(Error.Validation("product.inactive", "Inactive products cannot be updated."));

        if (categoryId == Guid.Empty)
            return Result.Failure(Error.Validation("product.category_required", "Category is required."));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(Error.Validation("product.name_required", "Product name is required."));

        name = name.Trim();

        if (name.Length > 120)
            return Result.Failure(Error.Validation("product.name_too_long", "Product name must not exceed 120 characters."));

        if (origin is < 0 or > 8)
            return Result.Failure(Error.Validation("product.origin_invalid", "Origin must be between 0 and 8."));

        CategoryId = categoryId;
        Name = name;
        Description = description;
        UnitOfMeasure = string.IsNullOrWhiteSpace(unitOfMeasure) ? "UN" : unitOfMeasure;
        Ncm = ncm;
        Cest = cest;
        Origin = origin;

        return Result.Success();
    }

    public Result Disable()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("product.already_disabled", "Product is already inactive."));

        IsActive = false;
        return Result.Success();
    }

    public Result<ProductVariant> AddVariant(Guid tenantId, string? size, string? color,
        string? sku, string? barcode, decimal price, decimal? costPrice,
        decimal stockQuantity, decimal? minStockQuantity, bool isStockTracked)
    {
        size = string.IsNullOrWhiteSpace(size) ? null : size.Trim();
        color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();

        if (size is null && color is null)
            return Error.Validation("variant.size_or_color_required", "Each variant must have at least one of Size or Color.");

        if (price < 0)
            return Error.Validation("variant.price_negative", "Price must be zero or greater.");

        if (costPrice.HasValue && costPrice.Value < 0)
            return Error.Validation("variant.cost_price_negative", "Cost price must be zero or greater.");

        if (stockQuantity < 0)
            return Error.Validation("variant.stock_negative", "Stock quantity must be zero or greater.");

        if (HasDuplicateVariant(Guid.Empty, size, color))
            return Error.Conflict("variant.duplicate", "A variant with the same size and color already exists for this product.");

        var variant = new ProductVariant(Id, tenantId, size, color, sku, barcode,
            price, costPrice, stockQuantity, minStockQuantity, isStockTracked);
        Variants.Add(variant);
        return variant;
    }

    public Result RemoveVariant(Guid variantId)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            return Result.Failure(Error.NotFound("variant.not_found", "Variant not found for this product."));

        Variants.Remove(variant);
        return Result.Success();
    }

    public Result UpdateVariant(Guid variantId, string? size, string? color,
        string? sku, string? barcode, decimal price, decimal? costPrice,
        decimal stockQuantity, decimal? minStockQuantity, bool isStockTracked)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            return Result.Failure(Error.NotFound("variant.not_found", "Variant not found for this product."));

        size = string.IsNullOrWhiteSpace(size) ? null : size.Trim();
        color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();

        if (size is null && color is null)
            return Result.Failure(Error.Validation("variant.size_or_color_required", "Each variant must have at least one of Size or Color."));

        if (HasDuplicateVariant(variantId, size, color))
            return Result.Failure(Error.Conflict("variant.duplicate", "A variant with the same size and color already exists for this product."));

        return variant.Update(size, color, sku, barcode, price, costPrice, stockQuantity, minStockQuantity, isStockTracked);
    }

    public Result AssignTags(IEnumerable<Guid> categoryIds)
    {
        Tags.Clear();
        foreach (var categoryId in categoryIds.Distinct())
            Tags.Add(new ProductTag(Id, TenantId, categoryId));
        return Result.Success();
    }

    public Result<ProductPhoto> AddPhoto(Guid tenantId, string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return Error.Validation("photo.url_required", "Photo URL is required.");

        if (url.Length > 2048)
            return Error.Validation("photo.url_too_long", "Photo URL must not exceed 2048 characters.");

        var photo = new ProductPhoto(Id, tenantId, url);
        Photos.Add(photo);
        return photo;
    }

    public Result RemovePhoto(Guid photoId)
    {
        var photo = Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo is null)
            return Result.Failure(Error.NotFound("photo.not_found", "Photo not found for this product."));

        if (photo.IsCover)
            return Result.Failure(Error.Validation("photo.cover_deletion_blocked",
                "Cannot delete the cover photo. Set another photo as cover first."));

        Photos.Remove(photo);
        return Result.Success();
    }

    public Result SetCoverPhoto(Guid photoId)
    {
        var photo = Photos.FirstOrDefault(p => p.Id == photoId);
        if (photo is null)
            return Result.Failure(Error.NotFound("photo.not_found", "Photo not found for this product."));

        foreach (var existing in Photos.Where(p => p.IsCover))
            existing.UnsetCover();

        photo.SetAsCover();
        return Result.Success();
    }

    private bool HasDuplicateVariant(Guid excludeVariantId, string? size, string? color) =>
        Variants.Any(v =>
            v.Id != excludeVariantId &&
            string.Equals(v.Size, size, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(v.Color, color, StringComparison.OrdinalIgnoreCase));
}
