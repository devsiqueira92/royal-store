using RoyalStore.Application.Sales.DTOs;
using RoyalStore.Domain.Catalog;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Fiscal;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.Commands.RegisterSale;

/// <summary>
/// Turns a lean checkout line into a complete, self-contained sale line by reading everything the
/// terminal did not send from the loaded catalog graph and the published tax table. Nothing
/// descriptive, priced or fiscal is ever taken from the request.
/// </summary>
public static class SaleItemEnricher
{
    /// <summary>CST origins (Tabela B) whose IBPT federal share comes from <c>NationalRate</c>;
    /// every other origin (1, 2, 6, 7, 8) uses <c>ImportedFederalRate</c> instead.</summary>
    private static readonly HashSet<string> NationalOrigins = ["0", "3", "4", "5"];

    public static Result<SaleItem> Enrich(
        RegisterSaleItem line,
        Guid tenantId,
        Guid saleId,
        Product product,
        ProductVariant variant,
        IbptTax? ibptTax)
    {
        var fiscalProfile = product.ResolveFiscalProfile();
        if (fiscalProfile.IsFailure)
            return fiscalProfile.Error;

        var catalog = new SaleItemCatalogSnapshot(
            VariantLabel: variant.BuildLabel(product),
            VariantSize: variant.Size,
            VariantColor: variant.Color,
            Sku: variant.Sku ?? product.Sku,
            OptionName: variant.OptionName,
            UnitMeasure: product.UnitOfMeasure);

        var fiscal = new SaleItemFiscalSnapshot(
            Origin: product.Origin,
            Ncm: product.Ncm,
            Cest: product.Cest,
            Cfop: fiscalProfile.Value.Cfop,
            IcmsCst: fiscalProfile.Value.IcmsCst,
            PisCofinsCst: fiscalProfile.Value.PisCofinsCst,
            IvaCst: fiscalProfile.Value.IvaCst,
            Nbs: product.Nbs,
            IssCode: product.IssCode,
            IsSelectiveTax: fiscalProfile.Value.IsSelectiveTax);

        // Only the rate the line's origin actually uses is kept — the other federal column
        // never applied to this item, so storing it would misrepresent which rate was charged.
        var isNational = NationalOrigins.Contains(product.Origin);
        var taxRates = ibptTax is null
            ? SaleItemTaxRates.None
            : new SaleItemTaxRates(
                isNational ? ibptTax.NationalRate : null,
                ibptTax.FederalRate,
                isNational ? null : ibptTax.ImportedFederalRate,
                ibptTax.StateRate,
                ibptTax.MunicipalRate);

        var itemResult = SaleItem.Create(
            id: Guid.NewGuid(),
            tenantId: tenantId,
            saleId: saleId,
            productId: product.Id,
            variantId: variant.Id,
            itemType: product.Type,
            productName: product.Name,
            productCode: variant.Barcode ?? product.Sku,
            quantity: line.Quantity,
            unitPrice: variant.EffectivePrice(product),
            costPrice: variant.Price ?? product.BasePrice,
            discountItem: 0m,
            catalog: catalog,
            fiscal: fiscal,
            taxRates: taxRates);

        if (itemResult.IsFailure)
            return Error.Validation(itemResult.Error.Code,
                $"Variant {variant.Id}: {itemResult.Error.Message}");

        return itemResult;
    }
}
