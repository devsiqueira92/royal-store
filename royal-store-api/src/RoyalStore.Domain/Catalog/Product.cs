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
    public string? Sku { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CostPrice { get; private set; }
    public decimal StockQuantity { get; private set; }
    public decimal? MinStockQuantity { get; private set; }
    public bool IsStockTracked { get; private set; }
    public bool IsActive { get; private set; }
    public string UnitOfMeasure { get; private set; } = "UN";
    public string? Barcode { get; private set; }
    public string? Ncm { get; private set; }
    public string? Cest { get; private set; }
    public int Origin { get; private set; }

    public ICollection<ProductSize> Sizes { get; private set; } = [];
    public ICollection<ProductPhoto> Photos { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Product() : base(Guid.NewGuid()) { }

    private Product(Guid id, Guid tenantId, Guid categoryId, string name, string? description,
        string? sku, decimal price, decimal? costPrice, decimal stockQuantity,
        decimal? minStockQuantity, bool isStockTracked,
        string unitOfMeasure, string? barcode, string? ncm, string? cest, int origin)
        : base(id)
    {
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        Description = description;
        Sku = sku;
        Price = price;
        CostPrice = costPrice;
        StockQuantity = stockQuantity;
        MinStockQuantity = minStockQuantity;
        IsStockTracked = isStockTracked;
        IsActive = true;
        UnitOfMeasure = unitOfMeasure;
        Barcode = barcode;
        Ncm = ncm;
        Cest = cest;
        Origin = origin;
    }

    public static Result<Product> Create(
        Guid tenantId, Guid categoryId, string name, string? description,
        string? sku, decimal price, decimal? costPrice, decimal stockQuantity,
        decimal? minStockQuantity, bool isStockTracked,
        string unitOfMeasure, string? barcode, string? ncm, string? cest, int origin)
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

        if (price < 0)
            return Error.Validation("product.price_negative", "Price must be zero or greater.");

        if (costPrice.HasValue && costPrice.Value < 0)
            return Error.Validation("product.cost_price_negative", "Cost price must be zero or greater.");

        if (stockQuantity < 0)
            return Error.Validation("product.stock_negative", "Stock quantity must be zero or greater.");

        if (origin is < 0 or > 8)
            return Error.Validation("product.origin_invalid", "Origin must be between 0 and 8.");

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            unitOfMeasure = "UN";

        return new Product(Guid.NewGuid(), tenantId, categoryId, name, description,
            sku, price, costPrice, stockQuantity, minStockQuantity, isStockTracked,
            unitOfMeasure, barcode, ncm, cest, origin);
    }

    public Result Update(
        Guid categoryId, string name, string? description,
        string? sku, decimal price, decimal? costPrice, decimal stockQuantity,
        decimal? minStockQuantity, bool isStockTracked,
        string unitOfMeasure, string? barcode, string? ncm, string? cest, int origin)
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

        if (price < 0)
            return Result.Failure(Error.Validation("product.price_negative", "Price must be zero or greater."));

        if (origin is < 0 or > 8)
            return Result.Failure(Error.Validation("product.origin_invalid", "Origin must be between 0 and 8."));

        CategoryId = categoryId;
        Name = name;
        Description = description;
        Sku = sku;
        Price = price;
        CostPrice = costPrice;
        StockQuantity = stockQuantity;
        MinStockQuantity = minStockQuantity;
        IsStockTracked = isStockTracked;
        UnitOfMeasure = string.IsNullOrWhiteSpace(unitOfMeasure) ? "UN" : unitOfMeasure;
        Barcode = barcode;
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

    public Result AssignSizes(IEnumerable<SizeValue> sizes)
    {
        var sizeList = sizes.ToList();

        if (sizeList.Contains(SizeValue.Osfa) && sizeList.Count > 1)
            return Result.Failure(Error.Validation("product.osfa_conflict", "OSFA cannot be combined with other sizes."));

        Sizes.Clear();

        foreach (var size in sizeList.Distinct())
            Sizes.Add(new ProductSize(Id, TenantId, size));

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
}
