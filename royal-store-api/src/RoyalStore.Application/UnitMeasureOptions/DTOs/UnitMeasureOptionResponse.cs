using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.UnitMeasureOptions.DTOs;

public record UnitMeasureOptionResponse(Guid Id, string Name)
{
    public static UnitMeasureOptionResponse From(UnitMeasureOption option) =>
        new(option.Id, $"{option.Code} — {option.Name}");
}
