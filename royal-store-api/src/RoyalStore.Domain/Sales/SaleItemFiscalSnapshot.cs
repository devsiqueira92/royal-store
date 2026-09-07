namespace RoyalStore.Domain.Sales;

/// <summary>
/// The fiscal classification captured on a sale line at sale time, after the product's
/// override-vs-tax-profile precedence has been resolved. A snapshot: later catalog or tax profile
/// edits never alter a stored sale.
/// </summary>
public sealed record SaleItemFiscalSnapshot(
    string Origin,
    string? Ncm,
    string? Cest,
    string? Cfop,
    string? IcmsCst,
    string? PisCofinsCst,
    string? IvaCst,
    string? Nbs,
    string? IssCode,
    bool IsSelectiveTax)
{
    public static readonly SaleItemFiscalSnapshot Empty =
        new(string.Empty, null, null, null, null, null, null, null, null, false);
}
