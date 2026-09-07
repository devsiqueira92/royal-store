using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Primitives;

namespace RoyalStore.Domain.Sales;

public sealed class SaleItem : Entity
{
    public Guid TenantId { get; private set; }
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid VariantId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string? ProductCode { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountItem { get; private set; }

    /// <summary>Quantity x unit price, less the line discount. Always computed, never supplied.</summary>
    public decimal TotalPrice { get; private set; }

    public SaleItemCatalogSnapshot Catalog { get; private set; } = SaleItemCatalogSnapshot.Empty;
    public SaleItemFiscalSnapshot Fiscal { get; private set; } = SaleItemFiscalSnapshot.Empty;
    public SaleItemTaxRates TaxRates { get; private set; } = SaleItemTaxRates.None;

    private SaleItem() : base(Guid.NewGuid()) { }

    private SaleItem(Guid id, Guid tenantId, Guid saleId, Guid productId, Guid variantId,
        string productName, string? productCode, decimal quantity, decimal unitPrice,
        decimal discountItem, decimal totalPrice,
        SaleItemCatalogSnapshot catalog, SaleItemFiscalSnapshot fiscal, SaleItemTaxRates taxRates)
        : base(id)
    {
        TenantId = tenantId;
        SaleId = saleId;
        ProductId = productId;
        VariantId = variantId;
        ProductName = productName;
        ProductCode = productCode;
        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountItem = discountItem;
        TotalPrice = totalPrice;
        Catalog = catalog;
        Fiscal = fiscal;
        TaxRates = taxRates;
    }

    public static Result<SaleItem> Create(
        Guid id, Guid tenantId, Guid saleId, Guid productId, Guid variantId, ItemType itemType,
        string productName, string? productCode, decimal quantity, decimal unitPrice, decimal discountItem,
        SaleItemCatalogSnapshot catalog, SaleItemFiscalSnapshot fiscal, SaleItemTaxRates taxRates)
    {
        if (id == Guid.Empty)
            return Error.Validation("saleitem.id_required", "Sale item ID is required.");

        if (tenantId == Guid.Empty)
            return Error.Validation("saleitem.tenant_required", "Tenant identifier is required.");

        if (saleId == Guid.Empty)
            return Error.Validation("saleitem.sale_required", "Sale identifier is required.");

        if (productId == Guid.Empty)
            return Error.Validation("saleitem.product_required", "Product identifier is required.");

        if (variantId == Guid.Empty)
            return Error.Validation("saleitem.variant_required", "Variant identifier is required.");

        if (quantity <= 0)
            return Error.Validation("saleitem.quantity_invalid", "Quantity must be greater than zero.");

        if (unitPrice < 0)
            return Error.Validation("saleitem.unit_price_negative", "Unit price cannot be negative.");

        if (discountItem < 0)
            return Error.Validation("saleitem.discount_negative", "Item discount cannot be negative.");

        var grossPrice = quantity * unitPrice;

        if (discountItem > grossPrice)
            return Error.Validation("saleitem.discount_exceeds_line",
                "Item discount cannot exceed the line's gross value.");

        if (string.IsNullOrWhiteSpace(productName))
            return Error.Validation("saleitem.product_name_required", "Product name is required.");

        productName = productName.Trim();
        if (productName.Length > 200)
            return Error.Validation("saleitem.product_name_too_long", "Product name must not exceed 200 characters.");

        if (productCode is not null && productCode.Length > 100)
            return Error.Validation("saleitem.product_code_too_long", "Product code must not exceed 100 characters.");

        var catalogResult = ValidateCatalog(catalog);
        if (catalogResult.IsFailure)
            return catalogResult.Error;

        var fiscalResult = ValidateFiscal(fiscal, itemType);
        if (fiscalResult.IsFailure)
            return fiscalResult.Error;

        var ratesResult = ValidateRates(taxRates);
        if (ratesResult.IsFailure)
            return ratesResult.Error;

        return new SaleItem(id, tenantId, saleId, productId, variantId, productName, productCode,
            quantity, unitPrice, discountItem, grossPrice - discountItem, catalog, fiscal, taxRates);
    }

    private static Result ValidateCatalog(SaleItemCatalogSnapshot catalog)
    {
        if (catalog is null)
            return Result.Failure(Error.Validation("saleitem.catalog_required", "Catalog snapshot is required."));

        if (string.IsNullOrWhiteSpace(catalog.VariantLabel))
            return Result.Failure(Error.Validation("saleitem.variant_label_required", "Variant label is required."));

        if (catalog.VariantLabel.Length > 200)
            return Result.Failure(Error.Validation("saleitem.variant_label_too_long", "Variant label must not exceed 200 characters."));

        if (catalog.VariantSize is { Length: > 50 })
            return Result.Failure(Error.Validation("saleitem.variant_size_too_long", "Variant size must not exceed 50 characters."));

        if (catalog.VariantColor is { Length: > 50 })
            return Result.Failure(Error.Validation("saleitem.variant_color_too_long", "Variant colour must not exceed 50 characters."));

        if (catalog.Sku is { Length: > 50 })
            return Result.Failure(Error.Validation("saleitem.sku_too_long", "SKU must not exceed 50 characters."));

        if (catalog.OptionName is { Length: > 100 })
            return Result.Failure(Error.Validation("saleitem.option_name_too_long", "Option name must not exceed 100 characters."));

        if (string.IsNullOrWhiteSpace(catalog.UnitMeasure))
            return Result.Failure(Error.Validation("saleitem.unit_measure_required", "Unit of measure is required."));

        if (catalog.UnitMeasure.Length > 36)
            return Result.Failure(Error.Validation("saleitem.unit_measure_too_long", "Unit of measure must not exceed 36 characters."));

        return Result.Success();
    }

    private static Result ValidateFiscal(SaleItemFiscalSnapshot fiscal, ItemType itemType)
    {
        if (fiscal is null)
            return Result.Failure(Error.Validation("saleitem.fiscal_required", "Fiscal snapshot is required."));

        if (string.IsNullOrWhiteSpace(fiscal.Origin))
            return Result.Failure(Error.Validation("saleitem.origin_required", "Origin is required."));

        if (fiscal.Origin.Length > 10)
            return Result.Failure(Error.Validation("saleitem.origin_too_long", "Origin must not exceed 10 characters."));

        if (itemType == ItemType.Product)
        {
            if (string.IsNullOrWhiteSpace(fiscal.Ncm))
                return Result.Failure(Error.Validation("saleitem.ncm_required", "NCM is required for goods."));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(fiscal.Nbs))
                return Result.Failure(Error.Validation("saleitem.nbs_required", "NBS is required for services."));

            if (string.IsNullOrWhiteSpace(fiscal.IssCode))
                return Result.Failure(Error.Validation("saleitem.iss_code_required", "ISS code is required for services."));
        }

        if (fiscal.Ncm is { Length: > 8 })
            return Result.Failure(Error.Validation("saleitem.ncm_too_long", "NCM must not exceed 8 characters."));

        if (fiscal.Cest is { Length: > 7 })
            return Result.Failure(Error.Validation("saleitem.cest_too_long", "CEST must not exceed 7 characters."));

        if (fiscal.Cfop is { Length: > 10 })
            return Result.Failure(Error.Validation("saleitem.cfop_too_long", "CFOP must not exceed 10 characters."));

        if (fiscal.IcmsCst is { Length: > 10 })
            return Result.Failure(Error.Validation("saleitem.icms_cst_too_long", "ICMS CST must not exceed 10 characters."));

        if (fiscal.PisCofinsCst is { Length: > 10 })
            return Result.Failure(Error.Validation("saleitem.pis_cofins_cst_too_long", "PIS/COFINS CST must not exceed 10 characters."));

        if (fiscal.IvaCst is { Length: > 10 })
            return Result.Failure(Error.Validation("saleitem.iva_cst_too_long", "IVA CST must not exceed 10 characters."));

        if (fiscal.Nbs is { Length: > 20 })
            return Result.Failure(Error.Validation("saleitem.nbs_too_long", "NBS must not exceed 20 characters."));

        if (fiscal.IssCode is { Length: > 20 })
            return Result.Failure(Error.Validation("saleitem.iss_code_too_long", "ISS code must not exceed 20 characters."));

        return Result.Success();
    }

    private static Result ValidateRates(SaleItemTaxRates rates)
    {
        if (rates is null)
            return Result.Failure(Error.Validation("saleitem.tax_rates_required", "Tax rate snapshot is required."));

        if (!IsRateInRange(rates.NationalRate) || !IsRateInRange(rates.FederalRate) ||
            !IsRateInRange(rates.ImportedFederalRate) || !IsRateInRange(rates.StateRate) ||
            !IsRateInRange(rates.MunicipalRate))
            return Result.Failure(Error.Validation("saleitem.rate_out_of_range",
                "Tax-burden rates must be between 0 and 100."));

        return Result.Success();
    }

    private static bool IsRateInRange(decimal? rate) => rate is null or (>= 0m and <= 100m);
}
