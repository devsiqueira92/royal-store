using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class Product : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string ReferenceCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public string UnitOfMeasure { get; private set; } = "UN";
    public string? Ncm { get; private set; }
    public string? Cest { get; private set; }
    public ProductOrigin Origin { get; private set; }
    public ItemType Type { get; private set; }
    public string? Nbs { get; private set; }
    public string? IssCode { get; private set; }
    public string? DefaultCfop { get; private set; }
    public string? IcmsCst { get; private set; }
    public string? PisCofinsCst { get; private set; }
    public string? IvaCst { get; private set; }
    public bool IsSelectiveTax { get; private set; }
    public int Version { get; private set; }

    public ICollection<ProductVariant> Variants { get; private set; } = [];
    public ICollection<ProductTag> Tags { get; private set; } = [];
    public ICollection<ProductPhoto> Photos { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Product() : base(Guid.NewGuid()) { }

    private Product(Guid id, Guid tenantId, Guid categoryId, string name, string referenceCode, string? description,
        string unitOfMeasure, string? ncm, string? cest, int origin,
        ItemType type, string? nbs, string? issCode, string? defaultCfop,
        string? icmsCst, string? pisCofinsCst, string? ivaCst, bool isSelectiveTax)
        : base(id)
    {
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        ReferenceCode = referenceCode;
        Description = description;
        IsActive = true;
        UnitOfMeasure = unitOfMeasure;
        Ncm = ncm;
        Cest = cest;
        Origin = (ProductOrigin)origin;
        Type = type;
        Nbs = Normalize(nbs);
        IssCode = Normalize(issCode);
        DefaultCfop = Normalize(defaultCfop);
        IcmsCst = Normalize(icmsCst);
        PisCofinsCst = Normalize(pisCofinsCst);
        IvaCst = Normalize(ivaCst);
        IsSelectiveTax = isSelectiveTax;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool HasNegativeMeasure(params decimal?[] values)
    {
        foreach (var value in values)
        {
            if (value is < 0)
                return true;
        }

        return false;
    }

    public static Result<Product> Create(
        Guid tenantId, Guid categoryId, string name, string referenceCode, string? description,
        string unitOfMeasure, string? ncm, string? cest, int origin,
        ItemType type, string? nbs, string? issCode, string? defaultCfop,
        string? icmsCst, string? pisCofinsCst, string? ivaCst, bool isSelectiveTax)
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

        if (string.IsNullOrWhiteSpace(referenceCode))
            return Error.Validation("product.reference_code_required", "Reference code is required.");

        referenceCode = referenceCode.Trim();

        if (referenceCode.Length > 50)
            return Error.Validation("product.reference_code_too_long", "Reference code must not exceed 50 characters.");

        if (description is not null && description.Length > 500)
            return Error.Validation("product.description_too_long", "Description must not exceed 500 characters.");

        if (origin is < 0 or > 8)
            return Error.Validation("product.origin_invalid", "Origin must be between 0 and 8.");

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            unitOfMeasure = "UN";

        return new Product(Guid.NewGuid(), tenantId, categoryId, name, referenceCode, description,
            unitOfMeasure, ncm, cest, origin,
            type, nbs, issCode, defaultCfop, icmsCst, pisCofinsCst, ivaCst, isSelectiveTax);
    }

    public Result Update(
        Guid categoryId, string name, string referenceCode, string? description,
        string unitOfMeasure, string? ncm, string? cest, int origin,
        ItemType type, string? nbs, string? issCode, string? defaultCfop,
        string? icmsCst, string? pisCofinsCst, string? ivaCst, bool isSelectiveTax)
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

        if (string.IsNullOrWhiteSpace(referenceCode))
            return Result.Failure(Error.Validation("product.reference_code_required", "Reference code is required."));

        referenceCode = referenceCode.Trim();

        if (referenceCode.Length > 50)
            return Result.Failure(Error.Validation("product.reference_code_too_long", "Reference code must not exceed 50 characters."));

        if (origin is < 0 or > 8)
            return Result.Failure(Error.Validation("product.origin_invalid", "Origin must be between 0 and 8."));

        CategoryId = categoryId;
        Name = name;
        ReferenceCode = referenceCode;
        Description = description;
        UnitOfMeasure = string.IsNullOrWhiteSpace(unitOfMeasure) ? "UN" : unitOfMeasure;
        Ncm = ncm;
        Cest = cest;
        Origin = (ProductOrigin)origin;
        Type = type;
        Nbs = Normalize(nbs);
        IssCode = Normalize(issCode);
        DefaultCfop = Normalize(defaultCfop);
        IcmsCst = Normalize(icmsCst);
        PisCofinsCst = Normalize(pisCofinsCst);
        IvaCst = Normalize(ivaCst);
        IsSelectiveTax = isSelectiveTax;
        Version++;

        return Result.Success();
    }

    public Result UpdateWithVariants(
        Guid categoryId, string name, string referenceCode, string? description,
        string unitOfMeasure, string? ncm, string? cest, int origin,
        ItemType type, string? nbs, string? issCode, string? defaultCfop,
        string? icmsCst, string? pisCofinsCst, string? ivaCst, bool isSelectiveTax,
        IReadOnlyList<NewVariantRequest> variantsToAdd,
        IReadOnlyList<VariantUpdateRequest> variantsToUpdate)
    {
        var fieldsResult = Update(categoryId, name, referenceCode, description, unitOfMeasure, ncm, cest, origin,
            type, nbs, issCode, defaultCfop, icmsCst, pisCofinsCst, ivaCst, isSelectiveTax);
        if (!fieldsResult.IsSuccess)
            return fieldsResult;

        foreach (var update in variantsToUpdate)
        {
            if (Variants.All(v => v.Id != update.VariantId))
                return Result.Failure(Error.NotFound("variant.not_found", $"Variant '{update.VariantId}' not found for this product."));
        }

        foreach (var update in variantsToUpdate)
        {
            var updateResult = UpdateVariant(
                update.VariantId, update.Size, update.Color, update.Sku, update.Barcode,
                update.Price, update.CostPrice, update.StockQuantity, update.MinStockQuantity, update.IsStockTracked,
                update.PromotionalPrice, update.NetWeight, update.GrossWeight,
                update.Width, update.Height, update.Length);
            if (!updateResult.IsSuccess)
                return Result.Failure(AttributeError(updateResult.Error, $"Variant '{update.VariantId}'"));
        }

        foreach (var add in variantsToAdd)
        {
            var addResult = AddVariant(
                TenantId, add.Size, add.Color, add.Sku, add.Barcode,
                add.Price, add.CostPrice, add.StockQuantity, add.MinStockQuantity, add.IsStockTracked,
                add.PromotionalPrice, add.NetWeight, add.GrossWeight,
                add.Width, add.Height, add.Length);
            if (!addResult.IsSuccess)
                return Result.Failure(AttributeError(addResult.Error, $"New variant (Size='{add.Size}', Color='{add.Color}')"));
        }

        return Result.Success();
    }

    private static Error AttributeError(Error error, string context)
    {
        var message = $"{context}: {error.Message}";
        return error.Type switch
        {
            ErrorType.Validation => Error.Validation(error.Code, message),
            ErrorType.NotFound => Error.NotFound(error.Code, message),
            ErrorType.Conflict => Error.Conflict(error.Code, message),
            ErrorType.Unauthorized => Error.Unauthorized(error.Code, message),
            ErrorType.RateLimit => Error.RateLimit(error.Code, message),
            _ => Error.Unexpected(error.Code, message)
        };
    }

    public Result Disable()
    {
        if (!IsActive)
            return Result.Failure(Error.Conflict("product.already_disabled", "Product is already inactive."));

        IsActive = false;
        Version++;
        return Result.Success();
    }

    public Result<ProductVariant> AddVariant(Guid tenantId, string? size, string? color,
        string? sku, string? barcode, decimal price, decimal? costPrice,
        decimal stockQuantity, decimal? minStockQuantity, bool isStockTracked,
        decimal? promotionalPrice, decimal? netWeight, decimal? grossWeight,
        decimal? width, decimal? height, decimal? length)
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

        if (promotionalPrice is < 0)
            return Error.Validation("variant.promotional_price_negative", "Promotional price must be zero or greater.");

        if (promotionalPrice.HasValue && promotionalPrice.Value > price)
            return Error.Validation("variant.promotional_price_above_price", "Promotional price must not exceed the regular price.");

        if (HasNegativeMeasure(netWeight, grossWeight, width, height, length))
            return Error.Validation("variant.measure_negative", "Weight and dimension values must be zero or greater.");

        if (HasDuplicateVariant(Guid.Empty, size, color))
            return Error.Conflict("variant.duplicate", "A variant with the same size and color already exists for this product.");

        var variant = new ProductVariant(Id, tenantId, size, color, sku, barcode,
            price, costPrice, stockQuantity, minStockQuantity, isStockTracked,
            promotionalPrice, netWeight, grossWeight, width, height, length);
        Variants.Add(variant);
        Version++;
        return variant;
    }

    public Result RemoveVariant(Guid variantId)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            return Result.Failure(Error.NotFound("variant.not_found", "Variant not found for this product."));

        if (Variants.Count == 1)
            return Result.Failure(Error.Validation("product.zero_variants", "A product must have at least one variant."));

        Variants.Remove(variant);
        Version++;
        return Result.Success();
    }

    public Result UpdateVariant(Guid variantId, string? size, string? color,
        string? sku, string? barcode, decimal price, decimal? costPrice,
        decimal stockQuantity, decimal? minStockQuantity, bool isStockTracked,
        decimal? promotionalPrice, decimal? netWeight, decimal? grossWeight,
        decimal? width, decimal? height, decimal? length)
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

        var updateResult = variant.Update(size, color, sku, barcode, price, costPrice, stockQuantity, minStockQuantity, isStockTracked,
            promotionalPrice, netWeight, grossWeight, width, height, length);
        if (updateResult.IsSuccess)
            Version++;

        return updateResult;
    }

    public Result AssignTags(IEnumerable<Guid> categoryIds)
    {
        Tags.Clear();
        foreach (var categoryId in categoryIds.Distinct())
            Tags.Add(new ProductTag(Id, TenantId, categoryId));
        Version++;
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
        Version++;
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
        Version++;
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
        Version++;
        return Result.Success();
    }

    private bool HasDuplicateVariant(Guid excludeVariantId, string? size, string? color) =>
        Variants.Any(v =>
            v.Id != excludeVariantId &&
            string.Equals(v.Size, size, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(v.Color, color, StringComparison.OrdinalIgnoreCase));
}
