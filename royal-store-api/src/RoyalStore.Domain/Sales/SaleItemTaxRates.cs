namespace RoyalStore.Domain.Sales;

/// <summary>
/// The IBPT tax-burden rates published for the line's classification and in force on the sale date.
/// Every rate is optional: a product may have no classification code, or no rates may be published
/// for it, and absence must stay distinguishable from a genuine zero rate.
/// </summary>
public sealed record SaleItemTaxRates(
    decimal? NationalRate,
    decimal? FederalRate,
    decimal? ImportedFederalRate,
    decimal? StateRate,
    decimal? MunicipalRate)
{
    public static readonly SaleItemTaxRates None = new(null, null, null, null, null);
}
