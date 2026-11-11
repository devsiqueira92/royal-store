using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Sales.DTOs;

public record SaleItemDetailsResponse(
    string ProductName,
    string? Sku,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string Origin,
    string? Ncm,
    string? Cest,
    string? Cfop,
    string? IcmsCst,
    string? PisCofinsCst,
    string? IvaCst,
    decimal? NationalRate,
    decimal? FederalRate,
    decimal? ImportedFederalRate,
    decimal? StateRate,
    decimal? MunicipalRate)
{
    public static SaleItemDetailsResponse From(SaleItem i) => new(
        i.ProductName,
        i.Catalog.Sku,
        i.Quantity,
        i.UnitPrice,
        i.TotalPrice,
        i.Fiscal.Origin,
        i.Fiscal.Ncm,
        i.Fiscal.Cest,
        i.Fiscal.Cfop,
        i.Fiscal.IcmsCst,
        i.Fiscal.PisCofinsCst,
        i.Fiscal.IvaCst,
        i.TaxRates.NationalRate,
        i.TaxRates.FederalRate,
        i.TaxRates.ImportedFederalRate,
        i.TaxRates.StateRate,
        i.TaxRates.MunicipalRate);
}
