namespace RoyalStore.Domain.Sales;

/// <summary>
/// The descriptive catalog values captured on a sale line at sale time. A snapshot: later catalog
/// edits never alter a stored sale.
/// </summary>
public sealed record SaleItemCatalogSnapshot(
    string VariantLabel,
    string? VariantSize,
    string? VariantColor,
    string? Sku,
    string? OptionName,
    string UnitMeasure)
{
    public static readonly SaleItemCatalogSnapshot Empty =
        new(string.Empty, null, null, null, null, "UN");
}
