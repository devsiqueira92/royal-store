using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Catalog;

public sealed class Product : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public string UnitOfMeasure { get; private set; } = "UN";
    public ItemType Type { get; private set; }
    public bool StockTrack { get; private set; }
    public decimal BasePrice { get; private set; }
    public decimal BaseCostPrice { get; private set; }
    public string? Ncm { get; private set; }
    public string? Cest { get; private set; }
    public string Origin { get; private set; }
    public string? Nbs { get; private set; }
    public string? IssCode { get; private set; }
    public string? DefaultCfop { get; private set; }
    public string? IcmsCst { get; private set; }
    public string? PisCofinsCst { get; private set; }
    public string? IvaCst { get; private set; }
    public bool? IsSelectiveTax { get; private set; }
    public Guid TaxProfileTemplateId { get; private set; }
    public TaxProfileTemplate TaxProfileTemplate { get; private set; } = null!;
    public bool OverrideTaxProfile { get; private set; }
    public ProductMeasure Measure { get; private set; } = ProductMeasure.Empty;
    public int Version { get; private set; }

    public ICollection<ProductVariant> Variants { get; private set; } = [];
    public ICollection<ProductPhoto> Photos { get; private set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    private Product() : base(Guid.NewGuid()) { }

    private Product(Guid id, Guid tenantId, Guid categoryId, string name, string sku, ItemType type,
        bool stockTrack, decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string? ncm, string? cest, string origin, string? nbs, string? issCode,
        Guid taxProfileTemplateId, bool overrideTaxProfile,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        ProductMeasure measure)
        : base(id)
    {
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        Sku = sku;
        Type = type;
        IsActive = true;
        StockTrack = stockTrack;
        BasePrice = basePrice;
        BaseCostPrice = baseCostPrice;
        Description = description;
        UnitOfMeasure = unitOfMeasure;
        Ncm = ncm;
        Cest = cest;
        Origin = origin;
        Nbs = nbs;
        IssCode = issCode;
        TaxProfileTemplateId = taxProfileTemplateId;
        OverrideTaxProfile = overrideTaxProfile;
        DefaultCfop = defaultCfop;
        IcmsCst = icmsCst;
        PisCofinsCst = pisCofinsCst;
        IvaCst = ivaCst;
        IsSelectiveTax = isSelectiveTax;
        Measure = measure;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result<(Guid TenantId, Guid CategoryId, string Name, string Sku, decimal BasePrice, decimal BaseCostPrice)>
        ValidateCommonFields(Guid tenantId, Guid categoryId, string name, string sku, decimal basePrice, decimal baseCostPrice)
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

        if (string.IsNullOrWhiteSpace(sku))
            return Error.Validation("product.sku_required", "SKU is required.");

        sku = sku.Trim();

        if (sku.Length > 50)
            return Error.Validation("product.sku_too_long", "SKU must not exceed 50 characters.");

        if (basePrice < 0)
            return Error.Validation("product.base_price_negative", "Base price must be zero or greater.");

        if (baseCostPrice < 0)
            return Error.Validation("product.base_cost_price_negative", "Base cost price must be zero or greater.");

        return (tenantId, categoryId, name, sku, basePrice, baseCostPrice);
    }

    private static Result ValidateTaxOverride(
        TaxProfileTemplate taxProfileTemplate, ItemType expectedTargetType, bool overrideTaxProfile,
        string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        string? defaultCfop = null, string? icmsCst = null, bool requireGoodsFields = false)
    {
        if (taxProfileTemplate is null)
            return Result.Failure(Error.Validation("product.tax_profile_template_required", "A tax profile template is required."));

        if (taxProfileTemplate.TargetItemType != expectedTargetType)
            return Result.Failure(Error.Conflict("product.tax_profile_template_kind_mismatch",
                $"The tax profile template must target {expectedTargetType}."));

        if (!overrideTaxProfile)
            return Result.Success();

        if (requireGoodsFields)
        {
            if (string.IsNullOrWhiteSpace(defaultCfop))
                return Result.Failure(Error.Validation("product.default_cfop_required", "Default CFOP is required when overriding the tax profile."));

            if (string.IsNullOrWhiteSpace(icmsCst))
                return Result.Failure(Error.Validation("product.icms_cst_required", "ICMS CST is required when overriding the tax profile."));
        }

        if (string.IsNullOrWhiteSpace(pisCofinsCst))
            return Result.Failure(Error.Validation("product.pis_cofins_cst_required", "PIS/COFINS CST is required when overriding the tax profile."));

        if (string.IsNullOrWhiteSpace(ivaCst))
            return Result.Failure(Error.Validation("product.iva_cst_required", "IVA CST is required when overriding the tax profile."));

        if (!isSelectiveTax.HasValue)
            return Result.Failure(Error.Validation("product.is_selective_tax_required", "The selective-tax flag is required when overriding the tax profile."));

        return Result.Success();
    }

    public static Result<Product> CreateProduct(
        Guid tenantId, Guid categoryId, string name, string sku, bool stockTrack,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string? ncm, string? cest, string origin,
        decimal? netWeight, decimal? grossWeight, decimal? width, decimal? height, decimal? length,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax)
    {
        var common = ValidateCommonFields(tenantId, categoryId, name, sku, basePrice, baseCostPrice);
        if (!common.IsSuccess)
            return common.Error;

        if (string.IsNullOrWhiteSpace(ncm))
            return Error.Validation("product.ncm_required", "NCM is required.");
        
        var measureResult = ProductMeasure.Create(netWeight, grossWeight, width, height, length);
        if (!measureResult.IsSuccess)
            return measureResult.Error;

        var overrideResult = ValidateTaxOverride(taxProfileTemplate, ItemType.Product, overrideTaxProfile,
            pisCofinsCst, ivaCst, isSelectiveTax, defaultCfop, icmsCst, requireGoodsFields: true);
        if (!overrideResult.IsSuccess)
            return overrideResult.Error;

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            unitOfMeasure = "UN";

        var (tid, cid, trimmedName, trimmedSku, bp, bcp) = common.Value;

        return new Product(Guid.NewGuid(), tid, cid, trimmedName, trimmedSku, ItemType.Product,
            stockTrack, bp, bcp, description, unitOfMeasure, Normalize(ncm), Normalize(cest), origin,
            null, null, taxProfileTemplate.Id, overrideTaxProfile,
            overrideTaxProfile ? Normalize(defaultCfop) : null,
            overrideTaxProfile ? Normalize(icmsCst) : null,
            overrideTaxProfile ? Normalize(pisCofinsCst) : null,
            overrideTaxProfile ? Normalize(ivaCst) : null,
            overrideTaxProfile ? isSelectiveTax : null,
            measureResult.Value)
        {
            TaxProfileTemplate = taxProfileTemplate
        };
    }

    public static Result<Product> CreateService(
        Guid tenantId, Guid categoryId, string name, string sku,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string nbs, string issCode,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax, string origin)
    {
        var common = ValidateCommonFields(tenantId, categoryId, name, sku, basePrice, baseCostPrice);
        if (!common.IsSuccess)
            return common.Error;

        if (string.IsNullOrWhiteSpace(nbs))
            return Error.Validation("product.nbs_required", "NBS is required.");

        if (string.IsNullOrWhiteSpace(issCode))
            return Error.Validation("product.iss_code_required", "ISS code is required.");

        var overrideResult = ValidateTaxOverride(taxProfileTemplate, ItemType.Service, overrideTaxProfile,
            pisCofinsCst, ivaCst, isSelectiveTax);
        if (!overrideResult.IsSuccess)
            return overrideResult.Error;

        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            unitOfMeasure = "UN";

        var (tid, cid, trimmedName, trimmedSku, bp, bcp) = common.Value;

        return new Product(Guid.NewGuid(), tid, cid, trimmedName, trimmedSku, ItemType.Service,
            stockTrack: false, bp, bcp, description, unitOfMeasure, ncm: null, cest: null, origin: origin,
            Normalize(nbs), Normalize(issCode), taxProfileTemplate.Id, overrideTaxProfile,
            defaultCfop: null, icmsCst: null,
            overrideTaxProfile ? Normalize(pisCofinsCst) : null,
            overrideTaxProfile ? Normalize(ivaCst) : null,
            overrideTaxProfile ? isSelectiveTax : null,
            ProductMeasure.Empty)
        {
            TaxProfileTemplate = taxProfileTemplate
        };
    }

    public Result UpdateProductFields(
        Guid categoryId, string name, string sku, bool stockTrack,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string? ncm, string? cest, string origin,
        decimal? netWeight, decimal? grossWeight, decimal? width, decimal? height, decimal? length,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax)
    {
        if (Type != ItemType.Product)
            return Result.Failure(Error.Validation("product.wrong_kind", "This catalog line is not a physical product."));

        if (!IsActive)
            return Result.Failure(Error.Validation("product.inactive", "Inactive products cannot be updated."));

        var common = ValidateCommonFields(TenantId, categoryId, name, sku, basePrice, baseCostPrice);
        if (!common.IsSuccess)
            return Result.Failure(common.Error);

        if (string.IsNullOrWhiteSpace(ncm))
            return Result.Failure(Error.Validation("product.ncm_required", "NCM is required."));


        var measureResult = ProductMeasure.Create(netWeight, grossWeight, width, height, length);
        if (!measureResult.IsSuccess)
            return Result.Failure(measureResult.Error);

        var overrideResult = ValidateTaxOverride(taxProfileTemplate, ItemType.Product, overrideTaxProfile,
            pisCofinsCst, ivaCst, isSelectiveTax, defaultCfop, icmsCst, requireGoodsFields: true);
        if (!overrideResult.IsSuccess)
            return overrideResult;

        var (_, cid, trimmedName, trimmedSku, bp, bcp) = common.Value;

        CategoryId = cid;
        Name = trimmedName;
        Sku = trimmedSku;
        StockTrack = stockTrack;
        BasePrice = bp;
        BaseCostPrice = bcp;
        Description = description;
        UnitOfMeasure = string.IsNullOrWhiteSpace(unitOfMeasure) ? "UN" : unitOfMeasure;
        Ncm = Normalize(ncm);
        Cest = Normalize(cest);
        Origin = origin;
        Measure = measureResult.Value;
        TaxProfileTemplateId = taxProfileTemplate.Id;
        TaxProfileTemplate = taxProfileTemplate;
        OverrideTaxProfile = overrideTaxProfile;
        DefaultCfop = overrideTaxProfile ? Normalize(defaultCfop) : null;
        IcmsCst = overrideTaxProfile ? Normalize(icmsCst) : null;
        PisCofinsCst = overrideTaxProfile ? Normalize(pisCofinsCst) : null;
        IvaCst = overrideTaxProfile ? Normalize(ivaCst) : null;
        IsSelectiveTax = overrideTaxProfile ? isSelectiveTax : null;
        Version++;

        return Result.Success();
    }

    public Result UpdateServiceFields(
        Guid categoryId, string name, string sku,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string nbs, string issCode,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax)
    {
        if (Type != ItemType.Service)
            return Result.Failure(Error.Validation("product.wrong_kind", "This catalog line is not a service."));

        if (!IsActive)
            return Result.Failure(Error.Validation("product.inactive", "Inactive services cannot be updated."));

        var common = ValidateCommonFields(TenantId, categoryId, name, sku, basePrice, baseCostPrice);
        if (!common.IsSuccess)
            return Result.Failure(common.Error);

        if (string.IsNullOrWhiteSpace(nbs))
            return Result.Failure(Error.Validation("product.nbs_required", "NBS is required."));

        if (string.IsNullOrWhiteSpace(issCode))
            return Result.Failure(Error.Validation("product.iss_code_required", "ISS code is required."));

        var overrideResult = ValidateTaxOverride(taxProfileTemplate, ItemType.Service, overrideTaxProfile,
            pisCofinsCst, ivaCst, isSelectiveTax);
        if (!overrideResult.IsSuccess)
            return overrideResult;

        var (_, cid, trimmedName, trimmedSku, bp, bcp) = common.Value;

        CategoryId = cid;
        Name = trimmedName;
        Sku = trimmedSku;
        BasePrice = bp;
        BaseCostPrice = bcp;
        Description = description;
        UnitOfMeasure = string.IsNullOrWhiteSpace(unitOfMeasure) ? "UN" : unitOfMeasure;
        Nbs = Normalize(nbs);
        IssCode = Normalize(issCode);
        TaxProfileTemplateId = taxProfileTemplate.Id;
        TaxProfileTemplate = taxProfileTemplate;
        OverrideTaxProfile = overrideTaxProfile;
        PisCofinsCst = overrideTaxProfile ? Normalize(pisCofinsCst) : null;
        IvaCst = overrideTaxProfile ? Normalize(ivaCst) : null;
        IsSelectiveTax = overrideTaxProfile ? isSelectiveTax : null;
        Version++;

        return Result.Success();
    }

    public Result UpdateProductWithVariants(
        Guid categoryId, string name, string sku, bool stockTrack,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string? ncm, string? cest, string origin,
        decimal? netWeight, decimal? grossWeight, decimal? width, decimal? height, decimal? length,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        IReadOnlyList<NewVariantRequest> variantsToAdd,
        IReadOnlyList<VariantUpdateRequest> variantsToUpdate)
    {
        var fieldsResult = UpdateProductFields(categoryId, name, sku, stockTrack, basePrice, baseCostPrice,
            description, unitOfMeasure, ncm, cest, origin, netWeight, grossWeight, width, height, length,
            taxProfileTemplate, overrideTaxProfile, defaultCfop, icmsCst, pisCofinsCst, ivaCst, isSelectiveTax);
        if (!fieldsResult.IsSuccess)
            return fieldsResult;

        return ApplyVariantChanges(variantsToAdd, variantsToUpdate);
    }

    public Result UpdateServiceWithVariants(
        Guid categoryId, string name, string sku,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string nbs, string issCode,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        IReadOnlyList<NewVariantRequest> variantsToAdd,
        IReadOnlyList<VariantUpdateRequest> variantsToUpdate)
    {
        var fieldsResult = UpdateServiceFields(categoryId, name, sku, basePrice, baseCostPrice,
            description, unitOfMeasure, nbs, issCode,
            taxProfileTemplate, overrideTaxProfile, pisCofinsCst, ivaCst, isSelectiveTax);
        if (!fieldsResult.IsSuccess)
            return fieldsResult;

        return ApplyVariantChanges(variantsToAdd, variantsToUpdate);
    }

    private Result ApplyVariantChanges(
        IReadOnlyList<NewVariantRequest> variantsToAdd,
        IReadOnlyList<VariantUpdateRequest> variantsToUpdate)
    {
        foreach (var update in variantsToUpdate)
        {
            if (Variants.All(v => v.Id != update.VariantId))
                return Result.Failure(Error.NotFound("variant.not_found", $"Variant '{update.VariantId}' not found for this product."));
        }

        foreach (var update in variantsToUpdate)
        {
            var updateResult = UpdateVariant(update.VariantId, update.Size, update.Color, update.OptionName,
                update.Sku, update.Barcode, update.Price, update.CostPrice, update.StockQuantity, update.MinStockQuantity);
            if (!updateResult.IsSuccess)
                return Result.Failure(AttributeError(updateResult.Error, $"Variant '{update.VariantId}'"));
        }

        foreach (var add in variantsToAdd)
        {
            var addResult = AddVariant(TenantId, add.Size, add.Color, add.OptionName,
                add.Sku, add.Barcode, add.Price, add.CostPrice, add.StockQuantity, add.MinStockQuantity);
            if (!addResult.IsSuccess)
                return Result.Failure(AttributeError(addResult.Error, $"New variant"));
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


    public Result Enable()
    {
        if (IsActive)
            return Result.Failure(Error.Conflict("product.already_enabled", "Product is already enabled."));

        IsActive = true;
        Version++;
        return Result.Success();
    }

    public Result<ProductVariant> AddVariant(Guid tenantId, string? size, string? color, string? optionName,
        string? sku, string? barcode, decimal? price, decimal? costPrice,
        decimal? stockQuantity, decimal? minStockQuantity)
    {
        size = string.IsNullOrWhiteSpace(size) ? null : size.Trim();
        color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        optionName = string.IsNullOrWhiteSpace(optionName) ? null : optionName.Trim();

        if (Type == ItemType.Service)
        {
            size = null;
            color = null;

            if (optionName is null)
                return Error.Validation("variant.option_name_required", "Option name is required for a service variant.");
        }
        else if (size is null && color is null)
        {
            return Error.Validation("variant.size_or_color_required", "Each variant must have at least one of Size or Color.");
        }

        var validation = ProductVariant.Validate(StockTrack, BasePrice, BaseCostPrice,
            sku, barcode, price, costPrice, stockQuantity, minStockQuantity);
        if (!validation.IsSuccess)
            return validation.Error;

        if (HasDuplicateVariant(Guid.Empty, size, color, optionName))
            return Error.Conflict("variant.duplicate", Type == ItemType.Service
                ? "A variant with the same option name already exists for this service."
                : "A variant with the same size and color already exists for this product.");

        var variant = new ProductVariant(Id, tenantId, size, color, optionName, sku, barcode,
            price, costPrice, stockQuantity, minStockQuantity);
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

    public Result UpdateVariant(Guid variantId, string? size, string? color, string? optionName,
        string? sku, string? barcode, decimal? price, decimal? costPrice,
        decimal? stockQuantity, decimal? minStockQuantity)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            return Result.Failure(Error.NotFound("variant.not_found", "Variant not found for this product."));

        size = string.IsNullOrWhiteSpace(size) ? null : size.Trim();
        color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        optionName = string.IsNullOrWhiteSpace(optionName) ? null : optionName.Trim();

        if (Type == ItemType.Service)
        {
            size = null;
            color = null;

            if (optionName is null)
                return Result.Failure(Error.Validation("variant.option_name_required", "Option name is required for a service variant."));
        }
        else if (size is null && color is null)
        {
            return Result.Failure(Error.Validation("variant.size_or_color_required", "Each variant must have at least one of Size or Color."));
        }

        if (HasDuplicateVariant(variantId, size, color, optionName))
            return Result.Failure(Error.Conflict("variant.duplicate", Type == ItemType.Service
                ? "A variant with the same option name already exists for this service."
                : "A variant with the same size and color already exists for this product."));

        var updateResult = variant.Update(size, color, optionName, sku, barcode, price, costPrice,
            stockQuantity, minStockQuantity, StockTrack, BasePrice, BaseCostPrice);
        if (updateResult.IsSuccess)
            Version++;

        return updateResult;
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

    private bool HasDuplicateVariant(Guid excludeVariantId, string? size, string? color, string? optionName) =>
        Type == ItemType.Service
            ? Variants.Any(v =>
                v.Id != excludeVariantId &&
                string.Equals(v.OptionName, optionName, StringComparison.OrdinalIgnoreCase))
            : Variants.Any(v =>
                v.Id != excludeVariantId &&
                string.Equals(v.Size, size, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(v.Color, color, StringComparison.OrdinalIgnoreCase));
}
