namespace RoyalStore.Domain.Catalog;

/// <summary>
/// The fiscal classification actually in force for a product, after applying the
/// <see cref="Product.OverrideTaxProfile"/> precedence rule against its
/// <see cref="TaxProfileTemplate"/>. Produced by <see cref="Product.ResolveFiscalProfile"/>.
/// </summary>
public sealed record ProductFiscalProfile(
    string? Cfop,
    string? IcmsCst,
    string? PisCofinsCst,
    string? IvaCst,
    bool IsSelectiveTax);
