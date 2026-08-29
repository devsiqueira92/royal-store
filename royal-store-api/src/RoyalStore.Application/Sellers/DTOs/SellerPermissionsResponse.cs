namespace RoyalStore.Application.Sellers.DTOs;

public record SellerPermissionsResponse(
    Guid SellerId,
    string UserId,
    string Role,
    IReadOnlyList<string> Grants,
    IReadOnlyList<string> EffectivePermissions);
