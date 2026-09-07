namespace RoyalStore.Application.Sales.DTOs;

/// <summary>
/// Optional customer identification supplied at checkout. Accepted so the terminal's payload is
/// valid, but not persisted and not attached to the sale in this feature.
/// </summary>
public record RegisterSaleCustomer(
    string? Document,
    string? Name,
    string? Phone,
    bool TermsAccepted = false);
