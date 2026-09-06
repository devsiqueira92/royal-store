using RoyalStore.Domain.Catalog;

namespace RoyalStore.Application.OriginOptions.DTOs;

public record OriginOptionResponse(string Code, string Name, string Description)
{
    public static OriginOptionResponse From(OriginOption option) =>
        new(option.Code, $"{option.Code} — {option.Name}", option.Description);
}
