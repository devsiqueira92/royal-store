using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;
using RoyalStore.Domain.Suppliers;

namespace RoyalStore.Domain.Catalog;

public sealed class Product : Entity, IAuditableEntity
{
    public Guid TenantId { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public Guid? SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
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

    /// <summary>One sale price per price list (061). Services have none — they keep
    /// <see cref="BasePrice"/> as their single price.</summary>
    public ICollection<ProductPrice> Prices { get; private set; } = [];

    /// <summary>
    /// The product's cost composition (064): a list of free-text cost lines with an amount, created on
    /// its own and linked here by a product save that names it. Read navigation — the foreign key
    /// (<see cref="CostCompositionList.ProductId"/>) is on the list. Informative only:
    /// <see cref="BaseCostPrice"/> is never derived from it.
    /// </summary>
    public CostCompositionList? CostComposition { get; private set; }

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

    /// <param name="prices">One price per price list; the store's default list is mandatory (061 FR-010).</param>
    /// <param name="tenantLists">Every price list of the store, active or not.</param>
    /// <param name="costComposition">Optional cost composition list to link to the new product (064 FR-013).</param>
    public static Result<Product> CreateProduct(
        Guid tenantId, Guid categoryId, string name, string sku, bool stockTrack,
        IReadOnlyList<ProductPriceInput> prices, IReadOnlyList<PriceList> tenantLists,
        decimal baseCostPrice, string? description, string unitOfMeasure,
        string? ncm, string? cest, string origin,
        decimal? netWeight, decimal? grossWeight, decimal? width, decimal? height, decimal? length,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        Supplier? supplier = null,
        CostCompositionList? costComposition = null)
    {
        // The sale price is no longer an input: it comes from the price lists and is mirrored into
        // BasePrice by ApplyPrices below.
        var common = ValidateCommonFields(tenantId, categoryId, name, sku, 0m, baseCostPrice);
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

        var product = new Product(Guid.NewGuid(), tid, cid, trimmedName, trimmedSku, ItemType.Product,
            stockTrack, bp, bcp, description, unitOfMeasure, Normalize(ncm), Normalize(cest), origin,
            null, null, taxProfileTemplate.Id, overrideTaxProfile,
            overrideTaxProfile ? Normalize(defaultCfop) : null,
            overrideTaxProfile ? Normalize(icmsCst) : null,
            overrideTaxProfile ? Normalize(pisCofinsCst) : null,
            overrideTaxProfile ? Normalize(ivaCst) : null,
            overrideTaxProfile ? isSelectiveTax : null,
            measureResult.Value)
        {
            TaxProfileTemplate = taxProfileTemplate,
            SupplierId = supplier?.Id,
            Supplier = supplier
        };

        var pricesResult = product.ApplyPrices(prices, tenantLists);
        if (!pricesResult.IsSuccess)
            return pricesResult.Error;

        // Links the named list last, once everything else about the new product is valid (064 FR-013).
        var compositionResult = product.UseCostComposition(costComposition);
        if (!compositionResult.IsSuccess)
            return compositionResult.Error;

        return product;
    }

    public static Result<Product> CreateService(
        Guid tenantId, Guid categoryId, string name, string sku,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string nbs, string issCode,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax, string origin,
        Supplier? supplier = null)
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
            TaxProfileTemplate = taxProfileTemplate,
            SupplierId = supplier?.Id,
            Supplier = supplier
        };
    }

    public Result<ProductPriceChanges> UpdateProductFields(
        Guid categoryId, string name, string sku, bool stockTrack,
        IReadOnlyList<ProductPriceInput> prices, IReadOnlyList<PriceList> tenantLists,
        decimal baseCostPrice, string? description, string unitOfMeasure,
        string? ncm, string? cest, string origin,
        decimal? netWeight, decimal? grossWeight, decimal? width, decimal? height, decimal? length,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        Supplier? supplier = null)
    {
        if (Type != ItemType.Product)
            return Error.Validation("product.wrong_kind", "This catalog line is not a physical product.");

        if (!IsActive)
            return Error.Validation("product.inactive", "Inactive products cannot be updated.");

        var common = ValidateCommonFields(TenantId, categoryId, name, sku, BasePrice, baseCostPrice);
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

        // Last check before anything is assigned: the prices are validated as a whole and only then
        // applied, so a rejected price leaves the product untouched (061 FR-013). BasePrice is
        // written there too, as the mirror of the default-list price.
        var pricesResult = ApplyPrices(prices, tenantLists);
        if (!pricesResult.IsSuccess)
            return pricesResult.Error;

        var (_, cid, trimmedName, trimmedSku, _, bcp) = common.Value;

        CategoryId = cid;
        Name = trimmedName;
        Sku = trimmedSku;
        StockTrack = stockTrack;
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
        SupplierId = supplier?.Id;
        Supplier = supplier;
        Version++;

        return pricesResult.Value;
    }

    public Result UpdateServiceFields(
        Guid categoryId, string name, string sku,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string nbs, string issCode,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        Supplier? supplier = null)
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
        SupplierId = supplier?.Id;
        Supplier = supplier;
        Version++;

        return Result.Success();
    }

    public Result<ProductPriceChanges> UpdateProductWithVariants(
        Guid categoryId, string name, string sku, bool stockTrack,
        IReadOnlyList<ProductPriceInput> prices, IReadOnlyList<PriceList> tenantLists,
        decimal baseCostPrice, string? description, string unitOfMeasure,
        string? ncm, string? cest, string origin,
        decimal? netWeight, decimal? grossWeight, decimal? width, decimal? height, decimal? length,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? defaultCfop, string? icmsCst, string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        IReadOnlyList<NewVariantRequest> variantsToAdd,
        IReadOnlyList<VariantUpdateRequest> variantsToUpdate,
        CostCompositionList? costComposition,
        Supplier? supplier = null)
    {
        // Checked before anything is assigned, so a refused list leaves the product and every list
        // untouched (064 FR-017, FR-018).
        var compositionCheck = CheckCostComposition(costComposition);
        if (compositionCheck.IsFailure)
            return compositionCheck.Error;

        var fieldsResult = UpdateProductFields(categoryId, name, sku, stockTrack, prices, tenantLists, baseCostPrice,
            description, unitOfMeasure, ncm, cest, origin, netWeight, grossWeight, width, height, length,
            taxProfileTemplate, overrideTaxProfile, defaultCfop, icmsCst, pisCofinsCst, ivaCst, isSelectiveTax,
            supplier);
        if (!fieldsResult.IsSuccess)
            return fieldsResult.Error;

        var variantsResult = ApplyVariantChanges(variantsToAdd, variantsToUpdate);
        if (!variantsResult.IsSuccess)
            return variantsResult.Error;

        // Last: by now every other part of the update is valid, so the link cannot be left half-done.
        var compositionResult = UseCostComposition(costComposition);
        if (compositionResult.IsFailure)
            return compositionResult.Error;

        return fieldsResult.Value;
    }

    public Result UpdateServiceWithVariants(
        Guid categoryId, string name, string sku,
        decimal basePrice, decimal baseCostPrice, string? description, string unitOfMeasure,
        string nbs, string issCode,
        TaxProfileTemplate taxProfileTemplate, bool overrideTaxProfile,
        string? pisCofinsCst, string? ivaCst, bool? isSelectiveTax,
        IReadOnlyList<NewVariantRequest> variantsToAdd,
        IReadOnlyList<VariantUpdateRequest> variantsToUpdate,
        Supplier? supplier = null)
    {
        var fieldsResult = UpdateServiceFields(categoryId, name, sku, basePrice, baseCostPrice,
            description, unitOfMeasure, nbs, issCode,
            taxProfileTemplate, overrideTaxProfile, pisCofinsCst, ivaCst, isSelectiveTax,
            supplier);
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

    internal static Error AttributeError(Error error, string context)
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

    /// <summary>
    /// Replaces this product's prices with the submitted set (061 FR-012): submitted lists are
    /// created or updated; prices on active lists that were left out are removed; prices on
    /// inactive lists that were left out are kept. Nothing is changed unless every price is valid.
    /// </summary>
    public Result<ProductPriceChanges> SetPrices(
        IReadOnlyList<ProductPriceInput> prices, IReadOnlyList<PriceList> tenantLists)
    {
        var result = ApplyPrices(prices, tenantLists);
        if (result.IsSuccess)
            Version++;

        return result;
    }

    /// <summary>
    /// The non-mutating half of <see cref="UseCostComposition"/>: whether <paramref name="list"/> may
    /// become this product's cost composition (064 FR-017). <c>null</c> is always fine — it means
    /// "leave the composition as it is" (FR-015).
    /// </summary>
    private Result CheckCostComposition(CostCompositionList? list)
    {
        if (list is null)
            return Result.Success();

        // A list of another store is simply not a list this product can see (FR-017).
        if (list.TenantId != TenantId)
            return Result.Failure(Error.NotFound("costcomposition.not_found", "Cost composition was not found."));

        // Already this product's list: nothing to refuse and nothing to change (FR-018).
        if (CostComposition?.Id == list.Id)
            return Result.Success();

        if (list.ProductId is { } otherProductId && otherProductId != Id)
            return Result.Failure(Error.Conflict("costcomposition.already_linked",
                "That cost composition is already linked to another product."));

        return Result.Success();
    }

    /// <summary>
    /// Makes <paramref name="list"/> this product's cost composition (064 FR-013, FR-014, FR-016).
    /// The list this product had before, if any, stays stored and is left linked to no product.
    /// <para>
    /// Adds no <see cref="Version"/> bump of its own: every product update already bumps it, so an
    /// update that only switches the list is a real product change for the existing stale-save
    /// protection (FR-019).
    /// </para>
    /// </summary>
    private Result UseCostComposition(CostCompositionList? list)
    {
        var check = CheckCostComposition(list);
        if (check.IsFailure)
            return check;

        if (list is null || CostComposition?.Id == list.Id)
            return Result.Success();

        CostComposition?.Unlink();
        list.LinkTo(Id);
        CostComposition = list;

        return Result.Success();
    }

    /// <summary>
    /// Removes the price on one list. This is the only way to drop a price on an inactive list,
    /// which <see cref="SetPrices"/> deliberately leaves alone. The default-list price cannot go:
    /// it is the price the product sells at.
    /// </summary>
    public Result<ProductPrice> RemovePrice(Guid priceListId)
    {
        if (Type != ItemType.Product)
            return Error.Validation("product.wrong_kind", "This catalog line is not a physical product.");

        var price = Prices.FirstOrDefault(p => p.PriceListId == priceListId);
        if (price is null)
            return Error.NotFound("product.price_not_found", "This product has no price on that list.");

        if (price.PriceList is null)
            return Error.Unexpected("product.price_list_not_loaded",
                $"The price lists of product '{Name}' were not loaded.");

        if (price.PriceList.IsDefault)
            return Error.Validation("product.default_price_required",
                "The price on the default list is required and cannot be removed.");

        Prices.Remove(price);
        Version++;
        return price;
    }

    /// <summary>
    /// Re-points the legacy single price at this product's price on the store's (new) default list.
    /// Called for every product when the default list changes, so <see cref="BasePrice"/> — still
    /// read by sales, the POS and the offline catalog — keeps meaning "the default-list price"
    /// (061 FR-020).
    /// </summary>
    public Result MirrorDefaultPrice(PriceList defaultList)
    {
        if (Type != ItemType.Product)
            return Result.Failure(Error.Validation("product.wrong_kind", "This catalog line is not a physical product."));

        if (defaultList.TenantId != TenantId || !defaultList.IsDefault)
            return Result.Failure(Error.Validation("product.price_list_invalid", "That is not this store's default price list."));

        var price = Prices.FirstOrDefault(p => p.PriceListId == defaultList.Id);
        if (price is null)
            return Result.Failure(Error.Conflict("product.default_price_required",
                $"Product '{Name}' has no price on the default list."));

        BasePrice = price.SalePrice;
        return Result.Success();
    }

    /// <summary>
    /// The price this product sells for on <paramref name="list"/>: the price saved on that list; else,
    /// for a list with a percentage rule, what the rule suggests from the base-list price; else the
    /// default-list price. Null only when the product has no price to fall back on at all.
    /// Requires <see cref="Prices"/> loaded with their <see cref="ProductPrice.PriceList"/>.
    /// </summary>
    public decimal? SalePriceOn(PriceList list)
    {
        var stored = Prices.FirstOrDefault(p => p.PriceListId == list.Id);
        if (stored is not null)
            return stored.SalePrice;

        if (list.HasRule
            && Prices.FirstOrDefault(p => p.PriceListId == list.BasePriceListId) is { } basePrice
            && list.SuggestPrice(basePrice.SalePrice) is { } suggested)
            return suggested;

        return Prices.FirstOrDefault(p => p.PriceList.IsDefault)?.SalePrice;
    }

    /// <summary>
    /// Tells what the rule of a price's list suggests for this product today, and whether the stored
    /// price is that amount (062 FR-020). Reads only — a stored price is never changed by a rule.
    /// <para>
    /// <see cref="PriceRuleStatus.Differs"/> means "not what the rule gives now": the price was
    /// typed over, or the percentage or the base-list price changed since it was saved. Nothing is
    /// stored to tell those apart.
    /// </para>
    /// </summary>
    public PriceRuleEvaluation EvaluatePriceRule(ProductPrice price)
    {
        var list = price.PriceList;
        if (list is null || !list.HasRule)
            return PriceRuleEvaluation.None;

        // Nothing to calculate from: this product is not priced on the base list.
        var basePrice = Prices.FirstOrDefault(p => p.PriceListId == list.BasePriceListId);
        if (basePrice is null)
            return PriceRuleEvaluation.None;

        var suggested = list.SuggestPrice(basePrice.SalePrice);
        if (suggested is null)
            return PriceRuleEvaluation.None;

        return new PriceRuleEvaluation(
            suggested,
            price.SalePrice == suggested.Value ? PriceRuleStatus.Matches : PriceRuleStatus.Differs);
    }

    private Result<ProductPriceChanges> ApplyPrices(
        IReadOnlyList<ProductPriceInput> prices, IReadOnlyList<PriceList> tenantLists)
    {
        if (Type != ItemType.Product)
            return Error.Validation("product.wrong_kind", "This catalog line is not a physical product.");

        // Lists of another store are simply not this product's lists (FR-023).
        var lists = tenantLists.Where(l => l.TenantId == TenantId).ToDictionary(l => l.Id);

        var defaultList = lists.Values.FirstOrDefault(l => l.IsDefault);
        if (defaultList is null)
            return Error.Conflict("pricelist.default_missing",
                "This store has no default price list. Create or choose one before saving products.");

        if (prices.GroupBy(p => p.PriceListId).Any(g => g.Count() > 1))
            return Error.Validation("product.price_duplicate_list",
                "A product can have only one price per price list.");

        foreach (var input in prices)
        {
            if (!lists.TryGetValue(input.PriceListId, out var list))
                return Error.Validation("product.price_list_invalid",
                    $"Price list '{input.PriceListId}' was not found.");

            var validation = ProductPrice.Validate(input.SalePrice, input.MinPrice);
            if (!validation.IsSuccess)
                return Error.Validation(validation.Error.Code, $"Price list '{list.Name}': {validation.Error.Message}");

            // A price on an inactive list is frozen: it may be resubmitted unchanged, but not
            // created or given a new value until the list is reactivated.
            var stored = Prices.FirstOrDefault(p => p.PriceListId == list.Id);
            if (!list.IsActive && (stored is null || !stored.HasValues(input.SalePrice, input.MinPrice)))
                return Error.Validation("product.price_list_inactive",
                    $"Price list '{list.Name}' is inactive; its price cannot be set or changed.");
        }

        var defaultInput = prices.FirstOrDefault(p => p.PriceListId == defaultList.Id);
        if (defaultInput is null)
            return Error.Validation("product.default_price_required",
                $"A price on the default list '{defaultList.Name}' is required.");

        // Everything is valid — only now does the aggregate change (FR-013).
        var added = new List<ProductPrice>();
        var removed = new List<ProductPrice>();

        foreach (var input in prices)
        {
            var stored = Prices.FirstOrDefault(p => p.PriceListId == input.PriceListId);
            if (stored is not null)
            {
                stored.Change(input.SalePrice, input.MinPrice);
                continue;
            }

            var price = new ProductPrice(TenantId, Id, input.PriceListId, input.SalePrice, input.MinPrice);
            Prices.Add(price);
            added.Add(price);
        }

        var submitted = prices.Select(p => p.PriceListId).ToHashSet();
        foreach (var stored in Prices.ToList())
        {
            if (submitted.Contains(stored.PriceListId))
                continue;

            // Left out and on an active list: removed. Left out and on an inactive list: kept.
            if (lists.TryGetValue(stored.PriceListId, out var list) && list.IsActive)
            {
                Prices.Remove(stored);
                removed.Add(stored);
            }
        }

        // The legacy single price stays as the mirror of the default-list price for as long as
        // the column exists (FR-019, FR-020).
        BasePrice = defaultInput.SalePrice;

        return new ProductPriceChanges(added, removed);
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

        // A product variant has no sale price of its own — it is priced by the product's price
        // lists (061 FR-024). Only a service option keeps one.
        if (Type != ItemType.Service)
            price = null;

        var validation = ProductVariant.Validate(Type, StockTrack, BasePrice, BaseCostPrice,
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
            stockQuantity, minStockQuantity, Type, StockTrack, BasePrice, BaseCostPrice);
        if (updateResult.IsSuccess)
            Version++;

        return updateResult;
    }

    /// <summary>Debits a sold quantity from one of this product's variants (RegisterSale). Mutations
    /// to a variant's stock go through the owning aggregate so the Version bump that guards against
    /// concurrent oversell is never forgotten at a call site.</summary>
    public Result DecreaseVariantStock(Guid variantId, decimal quantity)
    {
        var variant = Variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            return Result.Failure(Error.NotFound("variant.not_found", "Variant not found for this product."));

        var result = variant.DecreaseStock(quantity);
        if (result.IsSuccess)
            Version++;

        return result;
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

    /// <summary>
    /// Returns the fiscal classification actually in force for this product. When
    /// <see cref="OverrideTaxProfile"/> is false the product's own CFOP/CST columns are stored as
    /// null and the values come from the tax profile template; the flag selects the whole source,
    /// it does not merge field by field.
    /// </summary>
    public Result<ProductFiscalProfile> ResolveFiscalProfile()
    {
        if (OverrideTaxProfile)
            return new ProductFiscalProfile(DefaultCfop, IcmsCst, PisCofinsCst, IvaCst, IsSelectiveTax ?? false);

        if (TaxProfileTemplate is null)
            return Error.Unexpected("product.tax_profile_not_loaded",
                $"The tax profile template for product '{Name}' was not loaded.");

        return new ProductFiscalProfile(
            TaxProfileTemplate.DefaultCfop,
            TaxProfileTemplate.IcmsCst,
            TaxProfileTemplate.PisCofinsCst,
            TaxProfileTemplate.IvaCst,
            TaxProfileTemplate.IsSelectiveTax);
    }

    /// <summary>
    /// Classifies this product's stock health from its variants. A product that doesn't track
    /// stock, or has no variants, is always <see cref="StockStatus.InStock"/>. Otherwise it takes
    /// the most critical status across all variants (<see cref="StockStatus.OutOfStock"/> beats
    /// <see cref="StockStatus.LowStock"/> beats <see cref="StockStatus.InStock"/>); a variant with a
    /// null <see cref="ProductVariant.StockQuantity"/> is untracked and never makes the product
    /// low/out of stock. This is the source-of-truth rule for the <c>stockStatus</c> query filter
    /// on <c>GET /api/products</c>, which must express the same logic as an EF-translatable LINQ
    /// predicate in <c>ProductRepository.GetAllAsync</c> instead of calling this method, because EF
    /// Core cannot translate an arbitrary method call into SQL. Keep both in sync.
    /// </summary>
    public StockStatus GetStockStatus()
    {
        if (!StockTrack || Variants.Count == 0)
            return StockStatus.InStock;

        if (Variants.Any(v => v.StockQuantity is <= 0))
            return StockStatus.OutOfStock;

        if (Variants.Any(v => v.StockQuantity.HasValue && v.MinStockQuantity.HasValue
            && v.StockQuantity.Value <= v.MinStockQuantity.Value))
            return StockStatus.LowStock;

        return StockStatus.InStock;
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
