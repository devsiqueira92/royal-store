using RoyalStore.Domain.Fiscal;

namespace RoyalStore.Application.Fiscal.DTOs;

public record IbptTaxResponse(
    Guid Id,
    string Code,
    string? Exception,
    int Type,
    string Description,
    decimal NationalRate,
    decimal FederalRate,
    decimal ImportedFederalRate,
    decimal StateRate,
    decimal MunicipalRate,
    DateOnly EffectiveStart,
    DateOnly EffectiveEnd,
    bool IsActive)
{
    public static IbptTaxResponse From(IbptTax e) =>
        new(
            e.Id,
            e.Code,
            e.Exception,
            e.Type,
            e.Description,
            e.NationalRate,
            e.FederalRate,
            e.ImportedFederalRate,
            e.StateRate,
            e.MunicipalRate,
            e.EffectiveStart,
            e.EffectiveEnd,
            e.IsActive);
}
